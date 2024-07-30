using System.Collections;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Extensions;

namespace Jailbreak.SpecialDay;

/// <summary>
///   A wrapper for multiple zones, allows for easier management for
///   multiple zones where you might want players to be able to be in
///   separate, distinct zones. (e.g. cells)
/// </summary>
/// <param name="zones"></param>
public class MultiZoneWrapper(IEnumerable<IZone>? zones = null)
  : IZone, IEnumerable<IZone> {
  private readonly IEnumerable<IZone> zones = zones ?? [];

  public IEnumerator<IZone> GetEnumerator() { return zones.GetEnumerator(); }
  IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

  public bool IsInsideZone(Vector position) {
    return zones.Any(zone => zone.IsInsideZone(position));
  }

  public float GetMinDistance(Vector position) {
    return zones.Min(zone => zone.GetMinDistance(position));
  }

  public Vector GetCenter() {
    return zones
     .OrderBy(zone
        => zones.Sum(z => z.GetCenter().DistanceSquared(zone.GetCenter())))
     .First()
     .GetCenter();
  }

  public IEnumerable<Vector> GetPoints() {
    return zones.SelectMany(zone => zone.GetPoints());
  }
}