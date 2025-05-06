using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Timers;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views.LastRequest;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.LastRequest.LastRequests;

public class BulletForBullet(BasePlugin plugin, IServiceProvider provider,
  CCSPlayerController prisoner, CCSPlayerController guard, bool magForMag)
  : TeleportingRequest(plugin,
    provider.GetRequiredService<ILastRequestManager>(), prisoner, guard) {
  private const string weaponName = "weapon_deagle";

  public static readonly FakeConVar<bool> KILL_BY_HEALTH = new(
    "css_jb_lr_b4b_kill_by_health",
    "If true, the player with the lowest health will die after 60 seconds.",
    true);

  private readonly ChatMenu chatMenu =
    new(magForMag ? "Mag for Mag" : "Shot for Shot");

  private readonly ILRB4BLocale msg =
    provider.GetRequiredService<ILRB4BLocale>();

  private int? whosShot, magSize;

  public override LRType Type
    => magForMag ? LRType.MAG_FOR_MAG : LRType.SHOT_FOR_SHOT;

  public override void Setup() {
    Plugin.RegisterEventHandler<EventWeaponFire>(OnWeaponFire);

    Prisoner.RemoveWeapons();
    Guard.RemoveWeapons();

    base.Setup();
    Execute();
  }

  public override void Execute() {
    State = LRState.PENDING;
    MenuManager.OpenChatMenu(Prisoner, chatMenu);

    Plugin.AddTimer(5, () => {
      if (State != LRState.PENDING) return;
      Guard.GiveNamedItem(weaponName);
      Prisoner.GiveNamedItem(weaponName);

      magSize = (magForMag ?
        Prisoner.GetWeaponBase(weaponName)!.VData?.MaxClip1 :
        1) ?? 1;
      Prisoner.GetWeaponBase(weaponName).SetAmmo(0, 0);
      Guard.GetWeaponBase(weaponName).SetAmmo(0, 0);
      var shooter = new Random().Next(2) == 0 ? Prisoner : Guard;
      whosShot = shooter.Slot;
      msg.PlayerGoesFirst(shooter).ToChat(Prisoner, Guard);
      shooter.GetWeaponBase(weaponName).SetAmmo(magSize.Value, 0);
      State = LRState.ACTIVE;
    }, TimerFlags.STOP_ON_MAPCHANGE);

    Plugin.AddTimer(30, () => {
      if (State != LRState.ACTIVE) return;
      Prisoner.GiveNamedItem("weapon_knife");
      Guard.GiveNamedItem("weapon_knife");
    });

    if (!KILL_BY_HEALTH.Value) return;

    Plugin.AddTimer(60, () => {
      if (State != LRState.ACTIVE) return;
      var result = Guard.Health > Prisoner.Health ?
        LRResult.GUARD_WIN :
        LRResult.PRISONER_WIN;
      if (Guard.Health == Prisoner.Health) {
        var winner = whosShot != Prisoner.Slot ? Prisoner : Guard;
        msg.WinByReason(winner, "equal health");

        result = whosShot == Prisoner.Slot ?
          LRResult.GUARD_WIN :
          LRResult.PRISONER_WIN;
      } else {
        msg.WinByHealth(result == LRResult.GUARD_WIN ? Guard : Prisoner);
      }

      if (result == LRResult.GUARD_WIN)
        Prisoner.Pawn.Value?.CommitSuicide(false, true);
      else
        Guard.Pawn.Value?.CommitSuicide(false, true);
    }, TimerFlags.STOP_ON_MAPCHANGE);
  }

  private HookResult OnWeaponFire(EventWeaponFire @event, GameEventInfo info) {
    if (State != LRState.ACTIVE) return HookResult.Continue;

    var player = @event.Userid;
    if (player == null || whosShot == null || !player.IsValid
      || magSize == null)
      return HookResult.Continue;

    if (player.Slot != Prisoner.Slot && player.Slot != Guard.Slot)
      return HookResult.Continue;
    if (player.Slot != whosShot) {
      PrintToParticipants(player.PlayerName + " cheated.");
      player.Pawn.Value?.CommitSuicide(false, true);
      return HookResult.Handled;
    }

    var bullets = player.GetWeaponBase(weaponName)?.Clip1 ?? 1;
    if (bullets > 1) return HookResult.Continue;

    var opponent = player.Slot == Prisoner.Slot ? Guard : Prisoner;
    whosShot = opponent.Slot;
    opponent.GetWeaponBase(weaponName)?.SetAmmo(magSize.Value, 0);
    return HookResult.Continue;
  }

  public override void OnEnd(LRResult result) {
    Plugin.DeregisterEventHandler<EventWeaponFire>(OnWeaponFire);
    State = LRState.COMPLETED;

    switch (result) {
      case LRResult.GUARD_WIN when Guard.IsValid:
        Guard.GiveNamedItem("weapon_knife");
        break;
      case LRResult.PRISONER_WIN when Prisoner.IsValid:
        Prisoner.GiveNamedItem("weapon_knife");
        break;
    }
  }

  public override bool PreventEquip(CCSPlayerController player,
    CCSWeaponBaseVData weapon) {
    if (State == LRState.PENDING) return false;
    if (player != Prisoner && player != Guard) return false;
    return !weapon.Name.Contains("knife") && !weapon.Name.Contains("bayonet");
  }
}