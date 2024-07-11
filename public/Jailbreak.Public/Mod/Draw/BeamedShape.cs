using System.Drawing;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Mod.Draw;

/// <summary>
///   Represents a shape that is drawn using many beam segments
/// </summary>
public abstract class BeamedShape(BasePlugin plugin, Vector position,
  int resolution) : DrawableShape(plugin, position), IColorable {
  protected readonly BeamLine?[] Beams = new BeamLine[resolution];
  protected Color Color = Color.White;
  protected int Resolution;

  // TODO: Add support for rotation across arbitrary axis

  public Color GetColor() { return Color; }

  public void SetColor(Color color) { Color = color; }

  public override void Remove() {
    for (var i = 0; i < Beams.Length; i++) {
      Beams[i]?.Remove();
      Beams[i] = null;
    }
  }
}