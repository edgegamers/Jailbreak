using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Draw.Enums;

namespace Jailbreak.Draw.Shapes;

/// <summary>
///   Heart shape definition (approximate).
///   Desmos visualization: https://www.desmos.com/calculator/heartshape
/// </summary>
public class HeartShapeDefinition : IBeamShapeDefinition {
  // Pre-baked heart shape with 20 points (unit radius)
  // Approximate heart curve using piecewise bezier-like points
  private static readonly (float x, float y)[] HEART_POINTS = [
    (0.000f, 0.500f),
    (0.200f, 0.700f),
    (0.400f, 0.800f),
    (0.600f, 0.700f),
    (0.700f, 0.500f),
    (0.650f, 0.300f),
    (0.550f, 0.100f),
    (0.400f, -0.100f),
    (0.250f, -0.300f),
    (0.100f, -0.450f),
    (0.000f, -0.550f),
    (-0.100f, -0.450f),
    (-0.250f, -0.300f),
    (-0.400f, -0.100f),
    (-0.550f, 0.100f),
    (-0.650f, 0.300f),
    (-0.700f, 0.500f),
    (-0.600f, 0.700f),
    (-0.400f, 0.800f),
    (-0.200f, 0.700f),
    (0.000f, 0.500f)
  ];

  public BeamShapeType ShapeType => BeamShapeType.HEART;
  public IReadOnlyList<(float x, float y)> UnitPoints => HEART_POINTS;
  public bool IsClosed => true;
  public float DefaultRadius => 30f;
  public float DefaultWidth => 1.5f;
}