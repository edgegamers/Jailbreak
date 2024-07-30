using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Extensions;

namespace Jailbreak.SpecialDay;

public class DistanceBorder(IEnumerable<Vector> origins, float tolerance)
  : IBorder {
  public static float WIDTH_CELL = 200 * 200;
  public static float WIDTH_MEDIUM_ROOM = 1000 * 1000;
  public static float WIDTH_LARGE_ROOM = 1500 * 1500;

  public bool IsInsideRegion(Vector position) {
    return origins.Any(origin => origin.DistanceSquared(position) < tolerance);
  }

  public bool IsInsiderBorder(Vector position) {
    return GetMinDistance(position) < tolerance;
  }

  public float GetMinDistance(Vector position) {
    return origins.Min(origin => origin.DistanceSquared(position));
  }

  public Vector GetCenter() {
    return origins
     .OrderBy(origin => origins.Sum(point => point.DistanceSquared(origin)))
     .First();
  }

  public IEnumerable<Vector> GetPoints() { return origins; }
}