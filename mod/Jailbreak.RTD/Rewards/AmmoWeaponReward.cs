using System.Diagnostics;
using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Extensions;

namespace Jailbreak.RTD.Rewards;

public class AmmoWeaponReward : WeaponReward {
  private readonly int primary, secondary;

  public AmmoWeaponReward(string weapon, int primary, int secondary) :
    base(weapon) {
    Trace.Assert(Tag.GUNS.Contains(weapon));
    this.primary   = primary;
    this.secondary = secondary;
  }

  public override bool GrantReward(CCSPlayerController player) {
    var success = base.GrantReward(player);
    if (!success) return false;

    player.GetWeaponBase(weapon).SetAmmo(primary, secondary);
    return true;
  }
}