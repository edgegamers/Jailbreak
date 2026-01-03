using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Draw.Enums;

namespace Jailbreak.Draw.Shapes;

/// <summary>
///   Regular pentagon shape definition.
///   Desmos visualization: https://www.desmos.com/calculator/3i60e4yq1f
/// </summary>
public class PentagonShapeDefinition : IBeamShapeDefinition {
  // Pre-baked pentagon with 5 points (unit radius)
  private static readonly (float x, float y)[] PENTAGON_POINTS = [
    (0.000f, 1.000f),   // Top
    (0.951f, 0.309f),   // Top-right
    (0.588f, -0.809f),  // Bottom-right
    (-0.588f, -0.809f), // Bottom-left
    (-0.951f, 0.309f)   // Top-left
  ];

  public BeamShapeType ShapeType => BeamShapeType.PENTAGON;
  public IReadOnlyList<(float x, float y)> UnitPoints => PENTAGON_POINTS;
  public bool IsClosed => true;
  public float DefaultRadius => 30f;
  public float DefaultWidth => 1.5f;
}