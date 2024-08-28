using System.Diagnostics;
using System.Reflection;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.RTD;

namespace Jailbreak.RTD.Rewards;

public class WeaponReward : IRTDReward {
  protected readonly string weapon;

  public WeaponReward(string weapon) {
    this.weapon = weapon;
    Trace.Assert(Tag.GUNS.Contains(weapon));
  }

  public string Name => weapon.GetFriendlyWeaponName();

  public string Description
    => "You won a"
      + (weapon.GetFriendlyWeaponName()[0].IsVowel() ? "n" : "" + " ")
      + weapon.GetFriendlyWeaponName() + " next round.";

  public bool CanGrantReward(CCSPlayerController player) {
    // No point in giving a weapon to someone on CT
    return player.Team == CsTeam.Terrorist;
  }

  public virtual bool GrantReward(CCSPlayerController player) {
    player.GiveNamedItem(weapon);
    return true;
  }
}