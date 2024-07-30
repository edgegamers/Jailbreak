using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.SpecialDay;

namespace Jailbreak.Zones;

public class CHZZoneFactory : IZoneFactory {
  public IZone CreateZone(IEnumerable<Vector> origins) {
    return new ConvexHullZone(origins.ToList());
  }
}