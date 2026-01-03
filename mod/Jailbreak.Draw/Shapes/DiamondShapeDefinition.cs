using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Draw.Enums;

namespace Jailbreak.Draw.Shapes;

/// <summary>
///   Diamond shape definition (rotated oblong square).
///   Desmos visualization: https://www.desmos.com/calculator/jpvpg5uqsh
/// </summary>
public class DiamondShapeDefinition : IBeamShapeDefinition {
  // Pre-baked diamond with 4 points (unit radius)
  private static readonly (float x, float y)[] DIAMOND_POINTS = [
    (0.000f, 1.000f),  // Top
    (0.750f, 0.000f),  // Right
    (0.000f, -1.000f), // Bottom
    (-0.750f, 0.000f)  // Left
  ];

  public BeamShapeType ShapeType => BeamShapeType.DIAMOND;
  public IReadOnlyList<(float x, float y)> UnitPoints => DIAMOND_POINTS;
  public bool IsClosed => true;
  public float DefaultRadius => 30f;
  public float DefaultWidth => 1.5f;
}