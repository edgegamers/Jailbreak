using System.Collections.Immutable;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.RTD;

namespace Jailbreak.RTD.Rewards;

public class CannotPickupReward : AbstractOnTickReward {
  private readonly ImmutableHashSet<string> blockedWeapons;

  public CannotPickupReward(BasePlugin plugin, WeaponType blocked) : this(
    plugin, blocked.GetItems().ToArray()) {
    NameShort = blocked.ToString().ToTitleCase();
  }

  public CannotPickupReward(BasePlugin plugin, params string[] weapons) : base(
    plugin) {
    blockedWeapons = weapons.ToImmutableHashSet();
    NameShort = string.Join(", ",
      blockedWeapons.Select(s => s.GetFriendlyWeaponName()));
  }

  public override string Name => $"Cannot Use {NameShort}";
  public string NameShort { get; }

  public override string Description
    => $"You will not be able to use {NameShort} next round.";

  override protected void tick(CCSPlayerController player) {
    if (!player.IsReal()) return;
    var pawn = player.PlayerPawn.Value;
    if (pawn == null || !pawn.IsValid) return;
    var weaponServices = pawn.WeaponServices;
    if (weaponServices == null) return;
    var activeWeapon = weaponServices.ActiveWeapon.Value;
    if (activeWeapon == null || !activeWeapon.IsValid) return;
    if (!blockedWeapons.Contains(activeWeapon.DesignerName)) return;
    activeWeapon.NextPrimaryAttackTick   = Server.TickCount + 500;
    activeWeapon.NextSecondaryAttackTick = Server.TickCount + 500;
  }
}