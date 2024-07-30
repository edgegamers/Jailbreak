using System.Collections;
using System.Drawing;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Zones;

namespace Jailbreak.Zones;

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

  public int Id { get; set; }

  public bool IsInsideZone(Vector position) {
    return zones.Any(zone => zone.IsInsideZone(position));
  }

  public float GetMinDistance(Vector position) {
    return zones.Min(zone => zone.GetMinDistance(position));
  }

  public Vector GetCenterPoint() {
    return zones
     .OrderBy(zone => zones.Sum(z
        => z.GetCenterPoint().DistanceSquared(zone.GetCenterPoint())))
     .First()
     .GetCenterPoint();
  }

  public IEnumerable<Vector> GetBorderPoints() {
    return zones.SelectMany(zone => zone.GetBorderPoints());
  }

  public IEnumerable<Vector> GetAllPoints() {
    return zones.SelectMany(zone => zone.GetAllPoints());
  }

  public void Draw(BasePlugin plugin, Color color, float lifetime,
    float width = 1) {
    foreach (var zone in zones) zone.Draw(plugin, color, lifetime, width);
  }
}