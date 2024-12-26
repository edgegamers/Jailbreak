using System.Diagnostics;
using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views.LastRequest;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;
using Jailbreak.Public.Mod.Weapon;
using Microsoft.Extensions.DependencyInjection;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.LastRequest.LastRequests;

public class GunToss(BasePlugin plugin, ILastRequestManager manager,
  IServiceProvider provider, CCSPlayerController prisoner,
  CCSPlayerController guard)
  : TeleportingRequest(plugin, manager, prisoner, guard), IDropListener {
  public override LRType Type => LRType.GUN_TOSS;

  private readonly ILRGunTossLocale locale =
    provider.GetRequiredService<ILRGunTossLocale>();

  private Timer? guardGunTimer, prisonerGunTimer;
  private Timer? guardGroundTimer, prisonerGroundTimer;
  private bool guardTossed, prisonerTossed;

  private readonly List<BeamLine> guardLines = [], prisonerLines = [];

  /// <summary>
  /// Null if no one has thrown a gun yet, negative if only one has thrown a gun,
  /// Positive if both have thrown a gun.
  /// </summary>
  private int? bothThrewTick;

  public override void Setup() {
    base.Setup();

    if (Guard.PlayerPawn.Value != null)
      Guard.PlayerPawn.Value.TakesDamage = false;
    Prisoner.RemoveWeapons();
    Guard.RemoveWeapons();

    Plugin.AddTimer(3, Execute);
  }

  public override void Execute() {
    Prisoner.GiveNamedItem("weapon_knife");
    Guard.GiveNamedItem("weapon_knife");
    Prisoner.GiveNamedItem("weapon_deagle");
    Guard.GiveNamedItem("weapon_deagle");
    Prisoner.GetWeaponBase("weapon_deagle").SetAmmo(2, 0);

    State = LRState.ACTIVE;
    followPlayer(Prisoner);
    if (Guard.Slot != Prisoner.Slot) followPlayer(Guard);

    Plugin.RegisterListener<Listeners.OnTick>(OnTick);
  }

  private void OnTick() {
    if (bothThrewTick > 0) return;
    if (Guard is { IsValid: true, PlayerPawn.IsValid: true }
      && Guard.PlayerPawn.Value != null) {
      if ((Guard.PlayerPawn.Value.Flags & (uint)PlayerFlags.FL_ONGROUND) != 0)
        onGround(Guard);
    }

    if (Prisoner is { IsValid: true, PlayerPawn.IsValid: true }
      && Prisoner.PlayerPawn.Value != null) {
      if ((Prisoner.PlayerPawn.Value.Flags & (uint)PlayerFlags.FL_ONGROUND)
        != 0)
        onGround(Prisoner);
    }
  }

  private void onGround(CCSPlayerController player) {
    Server.PrintToChatAll("OnGround");
    if (bothThrewTick > 0) return;
    if (player.Slot == Prisoner.Slot && prisonerTossed) return;
    if (player.Slot == Guard.Slot && guardTossed) return;
    var lines = player.Slot == Prisoner.Slot ? prisonerLines : guardLines;
    lines.ForEach(l => l.Remove());
    lines.Clear();
  }

  public override void OnEnd(LRResult result) {
    State = LRState.COMPLETED;
    Plugin.RemoveListener<Listeners.OnTick>(OnTick);
  }

  public void OnWeaponDrop(CCSPlayerController player, CCSWeaponBase weapon) {
    if (State != LRState.ACTIVE) return;

    bothThrewTick = bothThrewTick switch {
      null => -Server.TickCount,
      < 0  => Server.TickCount,
      _    => bothThrewTick
    };

    if (player == Prisoner)
      prisonerTossed = true;
    else
      guardTossed = true;

    if (Guard.Slot == Prisoner.Slot) bothThrewTick = Server.TickCount;

    followWeapon(player, weapon);

    if (player == Prisoner)
      prisonerGroundTimer?.Kill();
    else
      guardGroundTimer?.Kill();

    if (player != Guard) return;
    if (Guard.PlayerPawn.Value != null) {
      Plugin.AddTimer(3, () => {
        if (State != LRState.ACTIVE) return;
        Guard.PlayerPawn.Value.TakesDamage = true;
      });
    }
  }

  private void followPlayer(CCSPlayerController player) {
    var     lines    = player == Prisoner ? prisonerLines : guardLines;
    Vector? previous = null;
    var timer = Plugin.AddTimer(0.1f, () => {
      Debug.Assert(player.PlayerPawn.Value != null,
        "player.PlayerPawn.Value != null");
      if ((player.PlayerPawn.Value.Flags & (uint)PlayerFlags.FL_ONGROUND)
        != 0) {
        // Player is on the ground
        lines.ForEach(l => l.Remove());
        lines.Clear();
        return;
      }

      var position = player.PlayerPawn.Value?.AbsOrigin!.Clone()!
        + new Vector(0, 0, 64);

      if (previous != null) {
        var line = new BeamLine(Plugin, previous, position);
        line.SetColor(player == Prisoner ? Color.Red : Color.Blue);
        line.SetWidth(1f);
        line.Draw(25);
        lines.Add(line);
      }

      previous = position.Clone();
    }, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);

    if (player == Prisoner)
      prisonerGroundTimer = timer;
    else
      guardGroundTimer = timer;
  }

  private void followWeapon(CCSPlayerController player, CCSWeaponBase weapon) {
    var lines   = player.Slot == Prisoner.Slot ? prisonerLines : guardLines;
    var lastPos = lines.Count > 0 ? lines[^1].End : null;
    Debug.Assert(player.PlayerPawn.Value != null,
      "player.PlayerPawn.Value != null");
    var timer = Plugin.AddTimer(0.1f, () => {
      if (weapon.AbsOrigin == null || !weapon.IsValid) {
        if (player == Prisoner)
          prisonerGunTimer?.Kill();
        else
          guardGunTimer?.Kill();
        return;
      }

      if (lastPos != null && lastPos.DistanceSquared(weapon.AbsOrigin) == 0) {
        if (player.Slot == Prisoner.Slot)
          prisonerGunTimer?.Kill();
        else
          guardGunTimer?.Kill();

        var firstPos = lines[0].Position;
        locale.PlayerThrewGunDistance(player, lastPos.Distance(firstPos))
         .ToAllChat();
        return;
      }

      if (lastPos != null) {
        var line = new BeamLine(Plugin, lastPos, weapon.AbsOrigin);
        line.SetColor(player == Prisoner ? Color.DarkRed : Color.DarkBlue);
        line.SetWidth(0.5f);
        line.Draw(25);
        lines.Add(line);
      }

      lastPos = weapon.AbsOrigin.Clone();
    }, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);

    if (player == Prisoner)
      prisonerGunTimer = timer;
    else
      guardGunTimer = timer;
  }

  public override bool PreventEquip(CCSPlayerController player,
    CCSWeaponBase weapon) {
    if (player != Prisoner && player != Guard) return false;
    if (State == LRState.PENDING) return false;
    return bothThrewTick is null or < 0;
  }
}