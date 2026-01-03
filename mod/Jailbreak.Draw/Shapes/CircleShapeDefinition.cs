using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Draw.Enums;

namespace Jailbreak.Draw.Shapes;

/// <summary>
///   Circle shape definition with 16 segments.
///   Desmos visualization: https://www.desmos.com/calculator/oyqs1zptgg
/// </summary>
public class CircleShapeDefinition : IBeamShapeDefinition {
  // Pre-baked circle with 16 points (unit radius)
  private static readonly (float x, float y)[] CIRCLE_POINTS = [
    (1.000f, 0.000f), (0.924f, 0.383f), (0.707f, 0.707f), (0.383f, 0.924f),
    (0.000f, 1.000f), (-0.383f, 0.924f), (-0.707f, 0.707f), (-0.924f, 0.383f),
    (-1.000f, 0.000f), (-0.924f, -0.383f), (-0.707f, -0.707f),
    (-0.383f, -0.924f), (0.000f, -1.000f), (0.383f, -0.924f), (0.707f, -0.707f),
    (0.924f, -0.383f)
  ];

  public BeamShapeType ShapeType => BeamShapeType.CIRCLE;
  public IReadOnlyList<(float x, float y)> UnitPoints => CIRCLE_POINTS;
  public bool IsClosed => true;
  public float DefaultRadius => 30f;
  public float DefaultWidth => 1.5f;
}