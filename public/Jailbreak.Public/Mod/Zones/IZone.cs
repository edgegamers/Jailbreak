using System.Drawing;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Mod.Draw;

namespace Jailbreak.Public.Mod.Zones;

public interface IZone {
  public int Id { get; set; }
  bool IsInsideZone(Vector position);
  float GetMinDistanceSquared(Vector position);

  /// <summary>
  ///   Represents a valid center point that players are able to spawn into
  /// </summary>
  /// <returns></returns>
  public Vector GetCenterPoint();

  /// <summary>
  ///   Calculates the center of all points, may not be a valid spawn point
  /// </summary>
  /// <returns></returns>
  public Vector CalculateCenterPoint() {
    var points = GetBorderPoints().ToList();
    var x      = 0f;
    var y      = 0f;
    var z      = 0f;
    foreach (var point in points) {
      x += point.X;
      y += point.Y;
      z += point.Z;
    }

    return new Vector(x / points.Count, y / points.Count, z / points.Count);
  }

  /// <summary>
  ///   Get all points that make up the border
  /// </summary>
  /// <returns></returns>
  public IEnumerable<Vector> GetBorderPoints();

  /// <summary>
  ///   Get all points that make up the zone,
  ///   including those that are not part of the border
  /// </summary>
  /// <returns></returns>
  public IEnumerable<Vector> GetAllPoints();

  public void AddPoint(Vector point);

  /// <summary>
  ///   Calculate the area of the zone
  /// </summary>
  /// <returns></returns>
  public float GetArea() {
    // Shoelace formula
    var    points = GetBorderPoints().ToList();
    var    n      = points.Count;
    double area   = 0;

    for (var i = 0; i < n; i++) {
      var current = points[i];
      var next    = points[(i + 1) % n];
      area += current.X * next.Y - next.X * current.Y;
    }

    return (float)(Math.Abs(area) / 2.0f);
  }

  public void Draw(BasePlugin plugin, Color color, float lifetime,
    float width = 1) {
    // TODO: Add point_worldtext to show the points of the zone

    var points = GetBorderPoints().ToList();

    switch (points.Count) {
      case 0:
        return;
      case 1:
        var circle = new BeamCircle(plugin, points.First(), 30, 10);
        circle.SetColor(color);
        circle.Draw(lifetime);
        break;
      default:
        for (var i = 0; i < points.Count; i++) {
          var first  = points[i];
          var second = points[(i + 1) % points.Count];
          var line   = new BeamLine(plugin, first, second);
          line.SetWidth(width);
          line.SetColor(color);
          line.Draw(lifetime);
        }

        break;
    }
  }
}