using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.LastRequest.LastRequests;

public class MagForMag(BasePlugin plugin, ILastRequestManager manager,
  CCSPlayerController prisoner, CCSPlayerController guard)
  : WeaponizedRequest(plugin, manager, prisoner, guard) {
  private const int BULLET_COUNT = 7;
  private CCSPlayerController? whosShot;
  public override LRType Type => LRType.GUN_TOSS;

  public override void Setup() {
    Plugin.RegisterEventHandler<EventPlayerShoot>(OnPlayerShoot);
    base.Setup();

    whosShot = new Random().Next(2) == 0 ? Prisoner : Guard;
    PrintToParticipants(whosShot.PlayerName + " will shoot first.");
    Prisoner.GiveNamedItem("weapon_deagle");
    Guard.GiveNamedItem("weapon_deagle");

    var weapon = findWeapon(Prisoner, "weapon_deagle");
    if (weapon != null) setAmmoAmount(weapon, 0, 0);
    weapon = findWeapon(Guard, "weapon_deagle");
    if (weapon != null) setAmmoAmount(weapon, 0, 0);
  }

  private static CBasePlayerWeapon? findWeapon(CCSPlayerController player,
    string name) {
    if (!player.IsReal()) return null;

    var pawn = player.PlayerPawn.Value;

    if (pawn == null) return null;

    var weapons = pawn.WeaponServices?.MyWeapons;

    return weapons?.Select(weaponOpt => weaponOpt.Value)
     .OfType<CBasePlayerWeapon>()
     .FirstOrDefault(weapon => weapon.DesignerName.Contains(name));
  }

  private static void setAmmoAmount(CBasePlayerWeapon weapon, int primary,
    int reserve) {
    weapon.Clip1 = primary;
    Utilities.SetStateChanged(weapon, "CBasePlayerWeapon", "m_iClip1");
    weapon.Clip2 = reserve;
    Utilities.SetStateChanged(weapon, "CBasePlayerWeapon", "m_pReserveAmmo");
  }

  public override void Execute() {
    State = LRState.ACTIVE;
    var deagle = findWeapon(whosShot!, "weapon_deagle");
    if (deagle != null) setAmmoAmount(deagle, BULLET_COUNT, 0);

    Plugin.AddTimer(30, () => {
      if (State != LRState.ACTIVE) return;
      Prisoner.GiveNamedItem("weapon_knife");
      Guard.GiveNamedItem("weapon_knife");
    });
    Plugin.AddTimer(60, () => {
      if (State != LRState.ACTIVE) return;
      PrintToParticipants("Time's Up!");
      var result = Guard.Health > Prisoner.Health ?
        LRResult.GUARD_WIN :
        LRResult.PRISONER_WIN;
      if (Guard.Health == Prisoner.Health) {
        PrintToParticipants("Even health, since " + whosShot!.PlayerName
          + " had the shot last, they lose.");
        result = whosShot.Slot == Prisoner.Slot ?
          LRResult.GUARD_WIN :
          LRResult.PRISONER_WIN;
      } else { PrintToParticipants("Health was the deciding factor. "); }

      Manager.EndLastRequest(this, result);
      if (result == LRResult.GUARD_WIN)
        Prisoner.Pawn.Value?.CommitSuicide(false, true);
      else
        Guard.Pawn.Value?.CommitSuicide(false, true);
    }, TimerFlags.STOP_ON_MAPCHANGE);
  }

  public HookResult OnPlayerShoot(EventPlayerShoot @event, GameEventInfo info) {
    if (State != LRState.ACTIVE) return HookResult.Continue;

    var player = @event.Userid;
    if (!player.IsReal()) return HookResult.Continue;

    if (player!.Slot != Prisoner.Slot && player.Slot != Guard.Slot)
      return HookResult.Continue;

    var shootersDeagle = findWeapon(player, "weapon_deagle");
    if (shootersDeagle == null) return HookResult.Continue;

    if (shootersDeagle.Clip1 != 0) return HookResult.Continue;

    PrintToParticipants(player.PlayerName + " has shot.");
    var opponent = player.Slot == Prisoner.Slot ? Guard : Prisoner;
    opponent.PrintToChat("Your shot");
    var deagle = findWeapon(opponent, "weapon_deagle");
    if (deagle != null) setAmmoAmount(deagle, 0, BULLET_COUNT);
    whosShot = opponent;
    return HookResult.Continue;
  }

  public override void OnEnd(LRResult result) {
    Plugin.RemoveListener(OnPlayerShoot);
    State = LRState.COMPLETED;
  }
}