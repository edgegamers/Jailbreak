using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.RTD;

public interface IRewardGenerator : IReadOnlyCollection<(IRTDReward, float)> {
  IRTDReward GenerateReward(int slot);

  IRTDReward GenerateReward(CCSPlayerController player) {
    return GenerateReward(player.UserId ?? -1);
  }
}