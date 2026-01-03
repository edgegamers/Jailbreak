using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Draw.Enums;

namespace Jailbreak.Draw.Shapes;

/// <summary>
///   Equilateral triangle shape definition.
///   Desmos visualization: https://www.desmos.com/calculator/3fobk1olvp
/// </summary>
public class TriangleShapeDefinition : IBeamShapeDefinition {
  // Pre-baked equilateral triangle with 3 points (unit radius)
  private static readonly (float x, float y)[] TRIANGLE_POINTS = [
    (0.000f, 1.000f),  // Top
    (0.866f, -0.500f), // Bottom-right
    (-0.866f, -0.500f) // Bottom-left
  ];

  public BeamShapeType ShapeType => BeamShapeType.TRIANGLE;
  public IReadOnlyList<(float x, float y)> UnitPoints => TRIANGLE_POINTS;
  public bool IsClosed => true;
  public float DefaultRadius => 30f;
  public float DefaultWidth => 1.5f;
}