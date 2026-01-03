using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Draw.Enums;

namespace Jailbreak.Draw.Shapes;

/// <summary>
///   Regular hexagon shape definition.
///   Desmos visualization: https://www.desmos.com/calculator/zeexph2fku
/// </summary>
public class HexagonShapeDefinition : IBeamShapeDefinition {
  // Pre-baked hexagon with 6 points (unit radius)
  private static readonly (float x, float y)[] HEXAGON_POINTS = [
    (0.000f, 1.000f),   // Top
    (0.866f, 0.500f),   // Top-right
    (0.866f, -0.500f),  // Bottom-right
    (0.000f, -1.000f),  // Bottom
    (-0.866f, -0.500f), // Bottom-left
    (-0.866f, 0.500f)   // Top-left
  ];

  public BeamShapeType ShapeType => BeamShapeType.HEXAGON;
  public IReadOnlyList<(float x, float y)> UnitPoints => HEXAGON_POINTS;
  public bool IsClosed => true;
  public float DefaultRadius => 30f;
  public float DefaultWidth => 1.5f;
}