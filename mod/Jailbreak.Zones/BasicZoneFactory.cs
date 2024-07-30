using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Mod.Zones;

namespace Jailbreak.Zones;

public class BasicZoneFactory : IZoneFactory {
  public IZone CreateZone(IEnumerable<Vector> origins) {
    return new DistanceZone(origins.ToList(), 0);
  }
}