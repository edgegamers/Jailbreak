using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Draw.Enums;

namespace Jailbreak.Draw.Shapes;

/// <summary>
///   Square shape definition.
///   Desmos visualization: https://www.desmos.com/calculator/lirekvc8ku
/// </summary>
public class SquareShapeDefinition : IBeamShapeDefinition {
  // Pre-baked square (unit size)
  private static readonly (float x, float y)[] SQUARE_POINTS = [
    (1.0f, 1.0f), (-1.0f, 1.0f), (-1.0f, -1.0f), (1.0f, -1.0f)
  ];

  public BeamShapeType ShapeType => BeamShapeType.SQUARE;
  public IReadOnlyList<(float x, float y)> UnitPoints => SQUARE_POINTS;
  public bool IsClosed => true;
  public float DefaultRadius => 30f;
  public float DefaultWidth => 1.5f;
}