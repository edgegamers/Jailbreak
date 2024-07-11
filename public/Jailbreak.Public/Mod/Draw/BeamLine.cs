using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Mod.Draw;

public class BeamLine(BasePlugin plugin, Vector position, Vector end)
  : DrawableShape(plugin, position), IColorable {
  private CEnvBeam? beam;
  private Color color = Color.White;
  private float width = 1f;

  public void SetColor(Color _color) { color = _color; }

  public Color GetColor() { return color; }

  public void Move(Vector start, Vector end1) {
    Position = start;
    end      = end1;
  }

  public override void Draw() {
    Remove();
    var newBeam = Utilities.CreateEntityByName<CEnvBeam>("env_beam");
    if (newBeam == null) return;
    newBeam.RenderMode = RenderMode_t.kRenderTransColor;
    newBeam.Width      = width;
    newBeam.Render     = GetColor();

    newBeam.Teleport(Position, new QAngle(), new Vector());
    newBeam.EndPos.X = end.X;
    newBeam.EndPos.Y = end.Y;
    newBeam.EndPos.Z = end.Z;
    beam             = newBeam;

    Utilities.SetStateChanged(newBeam, "CBeam", "m_vecEndPos");
  }

  public override void Remove() {
    KillTimer?.Kill();
    if (beam != null && beam.IsValid) beam?.Remove();
    beam = null;
  }

  public void SetWidth(float _width) { width = _width; }

  public float GetWidth() { return width; }
}