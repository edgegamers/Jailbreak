using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Zones;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace Jailbreak.Zones;

public class DistanceZone(IList<Vector> origins, float tolerance) : IZone {
  public static readonly float WIDTH_CELL = 200 * 200;
  public static readonly float WIDTH_MEDIUM_ROOM = 1000 * 1000;
  public static readonly float WIDTH_LARGE_ROOM = 1500 * 1500;

  private readonly IList<Vector> origins = origins.ToList();

  private IList<Vector> cachedHull =
    ConvexHullUtil.ComputeConvexHull(origins).ToList();

  public int Id { get; set; }

  public bool IsInsideZone(Vector position) {
    if (ConvexHullUtil.IsInsideZone(position, cachedHull)) return true;
    var minDistance = GetMinDistanceSquared(position);
    return minDistance < tolerance;
  }

  public float GetMinDistanceSquared(Vector position) {
    return ConvexHullUtil.GetMinDistanceSquared(position, cachedHull);
  }

  public float GetMinDistance(Vector position) {
    return (float)Math.Sqrt(GetMinDistanceSquared(position));
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

    cachedHull = ConvexHullUtil.ComputeConvexHull(origins).ToList();
  }

  public bool IsInsideRegion(Vector position) {
    return origins.Any(origin => origin.DistanceSquared(position) < tolerance);
  }
}