using System.Diagnostics;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.RTD;
using Jailbreak.Validator;

namespace Jailbreak.RTD.Rewards;

public class WeaponReward : IRTDReward {
  protected readonly string weapon;
  protected CsTeam? requiredTeam;

  public WeaponReward(BasePlugin plugin, string weapon,
    CsTeam? requiredTeam = CsTeam.Terrorist) {
    this.requiredTeam = requiredTeam;
    this.weapon       = weapon;
    Trace.Assert(new ItemValidator().Validate(weapon, out var error), error);
  }

  public string Name => weapon.GetFriendlyWeaponName();

  public string Description
    => "You won a"
      + (weapon.GetFriendlyWeaponName()[0].IsVowel() ? "n" : "" + " ")
      + weapon.GetFriendlyWeaponName() + " next round.";

  public bool CanGrantReward(CCSPlayerController player) {
    if (requiredTeam == null) return true;
    return player.Team == requiredTeam;
  }

  public virtual bool GrantReward(CCSPlayerController player) {
    if (!player.IsValid) return false;
    player.GiveNamedItem(weapon);
    return true;
  }
}