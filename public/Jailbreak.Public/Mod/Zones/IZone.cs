using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.SpecialDay;

public interface IZone {
  bool IsInsideZone(Vector position);
  float GetMinDistance(Vector position);
  public Vector GetCenter();
  public IEnumerable<Vector> GetPoints();

  /// <summary>
  ///   Calculate the area of the zone
  /// </summary>
  /// <returns></returns>
  public float GetArea() {
    // Shoelace formula
    var    points = GetPoints().ToList();
    var    n      = points.Count;
    double area   = 0;

    for (var i = 0; i < n; i++) {
      var current = points[i];
      var next    = points[(i + 1) % n];
      area += current.X * next.Y - next.X * current.Y;
    }

    return (float)(Math.Abs(area) / 2.0f);
  }
}