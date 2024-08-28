using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Mod.RTD;

namespace Jailbreak.RTD.Rewards;

public class CreditReward(int credits) : IRTDReward {
  public string Name => credits + " Credit";

  public string Description
    => "You won " + credits + " credit" + (credits == 1 ? "" : "s")
      + (credits > 500 ? "!" : ".");

  public bool Enabled => false; // TODO: Implement

  public bool PrepareReward(int userid) {
    // TODO: When we do implement, set their credits here
    return true;
  }

  public bool GrantReward(CCSPlayerController player) {
    // We would have already set their credits in PrepareReward, so do nothing here
    return true;
  }
}