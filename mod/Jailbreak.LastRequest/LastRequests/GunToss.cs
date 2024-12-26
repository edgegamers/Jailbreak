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
  }

  public override void OnEnd(LRResult result) { State = LRState.COMPLETED; }

  public void OnWeaponDrop(CCSPlayerController player, CCSWeaponBase weapon) {
    if (State != LRState.ACTIVE) return;

    followWeapon(player, weapon);

    if (player != Guard) return;
    if (Guard.PlayerPawn.Value != null) {
      Plugin.AddTimer(3, () => {
        if (State != LRState.ACTIVE) return;
        Guard.PlayerPawn.Value.TakesDamage = true;
      });
    }
  }

  private Timer? guardTimer, prisonerTimer;

  private void followWeapon(CCSPlayerController player, CCSWeaponBase weapon) {
    Vector? lastPos = null;
    Debug.Assert(player.PlayerPawn.Value != null,
      "player.PlayerPawn.Value != null");
    var firstPos =
      (weapon.AbsOrigin ?? player.PlayerPawn.Value.AbsOrigin)!.Clone();
    var startTime = Server.TickCount;
    var timer = Plugin.AddTimer(0.1f, () => {
      if (weapon.AbsOrigin == null || !weapon.IsValid) {
        if (player == Prisoner)
          prisonerTimer?.Kill();
        else
          guardTimer?.Kill();
        return;
      }

      if (lastPos != null && lastPos.DistanceSquared(weapon.AbsOrigin) == 0) {
        if (player == Prisoner)
          prisonerTimer?.Kill();
        else
          guardTimer?.Kill();

        locale.PlayerThrewGunDistance(player, lastPos.Distance(firstPos))
         .ToAllChat();
        return;
      }

      if (lastPos != null) {
        var line = new BeamLine(Plugin, lastPos, weapon.AbsOrigin);
        line.SetColor(player == Prisoner ? Color.Red : Color.Blue);
        line.SetWidth(0.5f);
        line.Draw(15f);
      }

      lastPos = weapon.AbsOrigin.Clone();
    }, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);

    if (player == Prisoner)
      prisonerTimer = timer;
    else
      guardTimer = timer;
  }
}