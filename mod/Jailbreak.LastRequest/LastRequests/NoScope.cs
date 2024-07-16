using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.LastRequest.LastRequests;

public class NoScope(BasePlugin plugin, ILastRequestManager manager,
  CCSPlayerController prisoner, CCSPlayerController guard)
  : WeaponizedRequest(plugin, manager, prisoner, guard) {
  public override LRType Type => LRType.NO_SCOPE;

  public override void Setup() {
    base.Setup();

    Plugin.RegisterListener<Listeners.OnTick>(OnTick);
  }

  private void OnTick() {
    if (State != LRState.ACTIVE) return;

    if (!Prisoner.IsReal() || !Guard.IsReal()) return;

    if (Prisoner.PlayerPawn.Value == null || Guard.PlayerPawn.Value == null)
      return;
    disableScope(Prisoner);
    disableScope(Guard);
  }

  private void disableScope(CCSPlayerController player) {
    if (!player.IsReal()) return;
    var pawn = player.PlayerPawn.Value;
    if (pawn == null || !pawn.IsValid) return;
    var weaponServices = pawn.WeaponServices;
    if (weaponServices == null) return;
    var activeWeapon = weaponServices.ActiveWeapon.Value;
    if (activeWeapon == null || !activeWeapon.IsValid) return;
    activeWeapon.NextSecondaryAttackTick = Server.TickCount + 500;
  }

  public override void Execute() {
    PrintToParticipants("Go!");
    Prisoner.GiveNamedItem("weapon_ssg08");
    Guard.GiveNamedItem("weapon_ssg08");
    State = LRState.ACTIVE;

    Plugin.AddTimer(30, () => {
      if (State != LRState.ACTIVE) return;
      Prisoner.GiveNamedItem("weapon_knife");
      Guard.GiveNamedItem("weapon_knife");
    }, TimerFlags.STOP_ON_MAPCHANGE);

    Plugin.AddTimer(60, () => {
      if (State != LRState.ACTIVE) return;

      Manager.EndLastRequest(this,
        Guard.Health > Prisoner.Health ?
          LRResult.GUARD_WIN :
          LRResult.PRISONER_WIN);
    }, TimerFlags.STOP_ON_MAPCHANGE);
  }

  public override void OnEnd(LRResult result) {
    State = LRState.COMPLETED;
    Plugin.RemoveListener<Listeners.OnTick>(OnTick);

    if (result != LRResult.GUARD_WIN && result != LRResult.PRISONER_WIN) return;

    var winner = result == LRResult.GUARD_WIN ? Guard : Prisoner;

    winner.RemoveWeapons();
    winner.GiveNamedItem("weapon_knife");
    winner.GiveNamedItem("weapon_ak47");
  }
}