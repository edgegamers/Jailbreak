using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Zones;

namespace Jailbreak.Zones;

public class DistanceZone(IList<Vector> origins, float tolerance) : IZone {
  public static readonly float WIDTH_CELL = 200 * 200;
  public static readonly float WIDTH_MEDIUM_ROOM = 1000 * 1000;
  public static readonly float WIDTH_LARGE_ROOM = 1500 * 1500;

  private readonly IList<Vector> origins = origins.ToList();

  private IEnumerable<Vector> cachedHull =
    ConvexHullUtil.ComputeConvexHull(origins);

  public int Id { get; set; }

  public bool IsInsideZone(Vector position) {
    return ConvexHullUtil.IsInsideZone(position, cachedHull.ToList());
  }

  public float GetMinDistance(Vector position) {
    return origins.Min(origin => origin.DistanceSquared(position));
  }

  public Vector GetCenterPoint() {
    return origins
     .OrderBy(origin => origins.Sum(point => point.DistanceSquared(origin)))
     .First();
  }

  public IEnumerable<Vector> GetBorderPoints() { return cachedHull; }

  public IEnumerable<Vector> GetAllPoints() { return origins; }

  public void AddPoint(Vector point) {
    origins.Add(point);

    cachedHull = ConvexHullUtil.ComputeConvexHull(origins);
  }

  public bool IsInsideRegion(Vector position) {
    return origins.Any(origin => origin.DistanceSquared(position) < tolerance);
  }
}