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
  public override LRType type => LRType.GUN_TOSS;

  public override void Setup() {
    plugin.RegisterEventHandler<EventPlayerShoot>(OnPlayerShoot);
    base.Setup();

    whosShot = new Random().Next(2) == 0 ? prisoner : guard;
    PrintToParticipants(whosShot.PlayerName + " will shoot first.");
    prisoner.GiveNamedItem("weapon_deagle");
    guard.GiveNamedItem("weapon_deagle");

    var weapon = findWeapon(prisoner, "weapon_deagle");
    if (weapon != null) setAmmoAmount(weapon, 0, 0);
    weapon = findWeapon(guard, "weapon_deagle");
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
    state = LRState.Active;
    var deagle = findWeapon(whosShot!, "weapon_deagle");
    if (deagle != null) setAmmoAmount(deagle, BULLET_COUNT, 0);

    plugin.AddTimer(30, () => {
      if (state != LRState.Active) return;
      prisoner.GiveNamedItem("weapon_knife");
      guard.GiveNamedItem("weapon_knife");
    });
    plugin.AddTimer(60, () => {
      if (state != LRState.Active) return;
      PrintToParticipants("Time's Up!");
      var result = guard.Health > prisoner.Health ?
        LRResult.GuardWin :
        LRResult.PrisonerWin;
      if (guard.Health == prisoner.Health) {
        PrintToParticipants("Even health, since " + whosShot!.PlayerName
          + " had the shot last, they lose.");
        result = whosShot.Slot == prisoner.Slot ?
          LRResult.GuardWin :
          LRResult.PrisonerWin;
      } else { PrintToParticipants("Health was the deciding factor. "); }

      manager.EndLastRequest(this, result);
      if (result == LRResult.GuardWin)
        prisoner.Pawn.Value?.CommitSuicide(false, true);
      else
        guard.Pawn.Value?.CommitSuicide(false, true);
    }, TimerFlags.STOP_ON_MAPCHANGE);
  }

  public HookResult OnPlayerShoot(EventPlayerShoot @event, GameEventInfo info) {
    if (state != LRState.Active) return HookResult.Continue;

    var player = @event.Userid;
    if (!player.IsReal()) return HookResult.Continue;

    if (player!.Slot != prisoner.Slot && player.Slot != guard.Slot)
      return HookResult.Continue;

    var shootersDeagle = findWeapon(player, "weapon_deagle");
    if (shootersDeagle == null) return HookResult.Continue;

    if (shootersDeagle.Clip1 != 0) return HookResult.Continue;

    PrintToParticipants(player.PlayerName + " has shot.");
    var opponent = player.Slot == prisoner.Slot ? guard : prisoner;
    opponent.PrintToChat("Your shot");
    var deagle = findWeapon(opponent, "weapon_deagle");
    if (deagle != null) setAmmoAmount(deagle, 0, BULLET_COUNT);
    whosShot = opponent;
    return HookResult.Continue;
  }

  public override void OnEnd(LRResult result) {
    plugin.RemoveListener(OnPlayerShoot);
    state = LRState.Completed;
  }
}