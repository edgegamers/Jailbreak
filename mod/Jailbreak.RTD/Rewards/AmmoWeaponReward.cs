using System.Diagnostics;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Extensions;

namespace Jailbreak.RTD.Rewards;

public class AmmoWeaponReward : WeaponReward {
  private readonly int primary, secondary;

  public AmmoWeaponReward(string weapon, int primary, int secondary,
    CsTeam requiredTeam = CsTeam.Terrorist) : base(weapon, requiredTeam) {
    Trace.Assert(global::Tag.GUNS.Contains(weapon));
    this.primary   = primary;
    this.secondary = secondary;
  }

  public override bool GrantReward(CCSPlayerController player) {
    player.GiveNamedItem(weapon);
    player.GetWeaponBase(weapon).SetAmmo(primary, secondary);
    return true;
  }
}