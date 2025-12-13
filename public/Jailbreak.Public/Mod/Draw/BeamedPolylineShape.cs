using System.Drawing;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Mod.Draw.Enums;

namespace Jailbreak.Public.Mod.Draw;

/// <summary>
///   Generic beamed shape renderer that draws a 2D polyline in 3D space using beams.
/// </summary>
public class BeamedPolylineShape : BeamedShape {
  private readonly IBeamShapeDefinition shapeDefinition;
  private Vector[] offsets;
  private float width;

  public float Radius { get; private set; }

  public BeamedPolylineShape(BasePlugin plugin, Vector position,
    IBeamShapeDefinition shapeDefinition, float? radius = null,
    float? width = null) : base(plugin, position,
    shapeDefinition.UnitPoints.Count) {
    this.shapeDefinition = shapeDefinition;
    Radius               = radius ?? shapeDefinition.DefaultRadius;
    this.width           = width ?? shapeDefinition.DefaultWidth;

    offsets = generateOffsets();
  }

  private Vector[] generateOffsets() {
    var newOffsets = new Vector[shapeDefinition.UnitPoints.Count];
    for (var i = 0; i < shapeDefinition.UnitPoints.Count; i++) {
      var point = shapeDefinition.UnitPoints[i];
      // Convert 2D unit points to 3D offsets (XY plane, Z=0)
      newOffsets[i] = new Vector(point.x * Radius, point.y * Radius, 0);
    }

    return newOffsets;
  }

  public override void Draw() {
    var segmentCount =
      shapeDefinition.IsClosed ? offsets.Length : offsets.Length - 1;

    for (var i = 0; i < segmentCount; i++) {
      var beam  = Beams[i];
      var start = Position + offsets[i];
      var end   = Position + offsets[(i + 1) % offsets.Length];

      if (beam == null) {
        beam = new BeamLine(Plugin, start, end);
        beam.SetWidth(width);
        beam.SetColor(Color);
        beam.Draw();
        Beams[i] = beam;
      } else {
        beam.Move(start, end);
        beam.Update();
      }
    }
  }

  public void SetRadius(float radius) {
    Radius  = radius;
    offsets = generateOffsets();
  }

  public void SetWidth(float newWidth) {
    width = newWidth;
    foreach (var beam in Beams) { beam?.SetWidth(width); }
  }
}