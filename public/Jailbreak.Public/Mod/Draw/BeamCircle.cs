using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Mod.Draw;

/// <summary>
/// An implementation of BeamedShape that allows for drawing a circle
/// </summary>
public class BeamCircle : BeamedShape {
  private readonly BeamLine?[] lines;
  private Vector[] offsets;
  private float radius;

  public BeamCircle(BasePlugin plugin, Vector position, float radius,
    int resolution) : base(plugin, position, resolution) {
    this.radius = radius;
    lines       = new BeamLine[resolution];

    offsets = generateOffsets();
  }

  private float degToRadian(float d) { return (float)(d * (Math.PI / 180)); }

  private Vector[] generateOffsets() {
    var newOffsets = new Vector[lines.Length];
    var angle      = 360f / lines.Length;
    for (var i = 0; i < lines.Length; i++) {
      var x = radius * MathF.Cos(degToRadian(angle * i));
      var y = radius * MathF.Sin(degToRadian(angle * i));
      newOffsets[i] = new Vector(x, y, 0);
    }

    return newOffsets;
  }

  public override void Draw() {
    for (var i = 0; i < lines.Length; i++) {
      var line  = lines[i];
      var start = Position + offsets[i];
      var end   = Position + offsets[(i + 1) % offsets.Length];
      if (line == null) {
        line = new BeamLine(Plugin, start, end);
        line.SetColor(Color);
        line.Draw();
        lines[i] = line;
      } else {
        line.Move(start, end);
        line.Update();
      }
    }
  }

  public void SetRadius(float _radius) {
    radius  = _radius;
    offsets = generateOffsets();
  }
}