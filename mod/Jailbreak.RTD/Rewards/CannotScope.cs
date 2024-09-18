using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Extensions;

namespace Jailbreak.RTD.Rewards;

public class CannotScope(BasePlugin plugin) : AbstractOnTickReward(plugin) {
  public override string Name => "Cannot Scope";

  public override string? Description
    => "You will not be able to scope next round.";

  override protected void tick(CCSPlayerController player) {
    if (!player.IsReal()) return;
    var pawn = player.PlayerPawn.Value;
    if (pawn == null || !pawn.IsValid) return;
    var weaponServices = pawn.WeaponServices;
    if (weaponServices == null) return;
    var activeWeapon = weaponServices.ActiveWeapon.Value;
    if (activeWeapon == null || !activeWeapon.IsValid) return;
    if (!Tag.SNIPERS.Contains(activeWeapon.DesignerName)) return;
    activeWeapon.NextSecondaryAttackTick = Server.TickCount + 500;
  }
}