using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.RTD;

namespace Jailbreak.RTD.Rewards;

public class HPReward(int hp) : IRTDReward {
  public string Name => hp + " HP";
  public string Description => "You won " + hp + " HP next round.";

  public bool GrantReward(CCSPlayerController player) {
    player.SetHealth(hp);
    return true;
  }
}