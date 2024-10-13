using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Extensions;

namespace Jailbreak.RTD.Rewards;

public class CannotLeftKnife(BasePlugin plugin) : AbstractOnTickReward(plugin) {
  public override string Name => "Cannot Left-Knife";

  public override string Description
    => "You will not be able to left-click knife next round.";

  override protected void tick(CCSPlayerController player) {
    if (!player.IsReal()) return;
    var pawn = player.PlayerPawn.Value;
    if (pawn == null || !pawn.IsValid) return;
    var weaponServices = pawn.WeaponServices;
    if (weaponServices == null) return;
    var activeWeapon = weaponServices.ActiveWeapon.Value;
    if (activeWeapon == null || !activeWeapon.IsValid) return;
    if (activeWeapon.DesignerName != "weapon_knife") return;
    activeWeapon.NextPrimaryAttackTick = Server.TickCount + 500;
  }
}