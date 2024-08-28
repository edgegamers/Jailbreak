using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.RTD;

/// <summary>
///   A rewarder that manages rewards for players.
/// </summary>
public interface IRTDRewarder {
  /// <summary>
  ///   True if the player currently has a reward.
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  bool HasReward(int id) { return GetReward(id) != null; }

  bool HasReward(CCSPlayerController player) {
    return HasReward(player.UserId ?? -1);
  }

  /// <summary>
  ///   Gets the reward for the player.
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  IRTDReward? GetReward(int id);

  IRTDReward? GetReward(CCSPlayerController player) {
    return GetReward(player.UserId ?? -1);
  }

  /// <summary>
  ///   Attempts to give a reward to the player.
  /// </summary>
  /// <param name="id"></param>
  /// <param name="reward"></param>
  /// <returns></returns>
  bool SetReward(int id, IRTDReward reward);

  bool SetReward(CCSPlayerController player, IRTDReward reward) {
    return SetReward(player.UserId ?? -1, reward);
  }
}