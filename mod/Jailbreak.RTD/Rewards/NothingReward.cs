using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Mod.RTD;

namespace Jailbreak.RTD.Rewards;

public class NothingReward : IRTDReward {
  public string Name => "Nothing";
  public string Description => "You won nothing.";
  public bool GrantReward(CCSPlayerController player) { return true; }
}