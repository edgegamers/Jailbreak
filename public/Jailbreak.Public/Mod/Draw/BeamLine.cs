using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Extensions;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace Jailbreak.Public.Mod.Draw;

/// <summary>
///   An implementation of DrawableShape that uses a CEnvBeam to draw a line
/// </summary>
/// <param name="plugin"></param>
/// <param name="position"></param>
/// <param name="end"></param>
public class BeamLine(BasePlugin plugin, Vector position, Vector end)
  : DrawableShape(plugin, position), IColorable {
  private CEnvBeam? beam;
  private Color color = Color.White;
  private float width = 1f;
  public Vector End => end.Clone();

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
    // Force a pure two-point (world-space) beam. With no explicit BeamType the
    // engine treats a code-spawned env_beam as entity-anchored and walks
    // m_pSceneNode->m_pParent->m_pOwner to resolve a scene owner that never
    // exists here, hanging the server's main thread in an infinite loop
    // (Valve csgo-osx-linux #4530 "CEnvBeam infinite loop" -> watchdog kill).
    // BEAM_POINTS draws origin->EndPos and skips that scene-owner resolution.
    newBeam.BeamType   = BeamType_t.BEAM_POINTS;
    newBeam.RenderMode = RenderMode_t.kRenderTransAlpha;
    newBeam.Width      = width;
    newBeam.Render     = GetColor();

    newBeam.Teleport(Position, new QAngle(), new Vector());
    newBeam.EndPos.X = End.X;
    newBeam.EndPos.Y = End.Y;
    newBeam.EndPos.Z = End.Z;
    beam             = newBeam;

    Utilities.SetStateChanged(newBeam, "CBeam", "m_nBeamType");
    Utilities.SetStateChanged(newBeam, "CBeam", "m_vecEndPos");
  }

  public override void Remove() {
    KillTimer?.Kill();
    if (beam != null && beam.IsValid) beam.Remove();

    beam = null;
  }

  public void SetWidth(float _width) { width = _width; }

  public float GetWidth() { return width; }
}