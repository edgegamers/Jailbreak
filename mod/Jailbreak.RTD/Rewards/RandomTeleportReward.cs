using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Mod.RTD;
using Jailbreak.Public.Mod.Zones;
using Jailbreak.Public.Utils;

namespace Jailbreak.RTD.Rewards;

public class RandomTeleportReward(IZoneManager? zones) : IRTDReward {
  public string Name => "Random Teleport";

  public string Description
    => "You will be teleported to a random location next round.";

  public bool Enabled => zones != null;

  public bool CanGrantReward(CCSPlayerController player) {
    return player.Team == CsTeam.Terrorist;
  }

  public bool GrantReward(CCSPlayerController player) {
    var zone = MapUtil.GetRandomSpawns(1, zones).FirstOrDefault();
    if (zone == null) return false;
    var pawn = player.Pawn.Value;
    if (pawn == null || !pawn.IsValid) return false;
    pawn.Teleport(zone);
    return true;
  }
}