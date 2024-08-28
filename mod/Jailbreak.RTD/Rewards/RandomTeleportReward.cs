using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Mod.RTD;
using Jailbreak.Public.Mod.Zones;
using Jailbreak.Public.Utils;

namespace Jailbreak.RTD.Rewards;

public class RandomTeleportReward(IZoneManager zones) : IRTDReward {
  public string Name => "Random Teleport";

  public string Description
    => "You will be teleported to a random location next round.";

  public bool GrantReward(CCSPlayerController player) {
    var zone = MapUtil.GetRandomSpawns(1, zones).FirstOrDefault();
    if (zone == null) return false;
    player.Teleport(zone);
    return true;
  }
}