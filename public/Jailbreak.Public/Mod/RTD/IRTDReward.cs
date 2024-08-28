using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.RTD;

public interface IRTDReward {
  public string Name { get; }
  public string? Description { get; }
  bool Enabled => true;

  bool CanGrantReward(int userid) {
    var player = Utilities.GetPlayerFromUserid(userid);
    return player != null && player.IsValid && CanGrantReward(player);
  }

  bool CanGrantReward(CCSPlayerController player) { return true; }

  bool PrepareReward(int userid) {
    var player = Utilities.GetPlayerFromUserid(userid);
    if (player == null || !player.IsValid) return false;
    return PrepareReward(player);
  }

  bool PrepareReward(CCSPlayerController player) { return true; }

  bool GrantReward(int userid) {
    var player = Utilities.GetPlayerFromUserid(userid);
    if (player == null || !player.IsValid) return false;
    return player.PawnIsAlive && GrantReward(player);
  }

  bool GrantReward(CCSPlayerController player);
}