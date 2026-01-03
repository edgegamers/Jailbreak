using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Draw.Enums;

namespace Jailbreak.Draw.Shapes;

/// <summary>
///   5-pointed star shape definition.
///   Desmos visualization: https://www.desmos.com/calculator/wvk2uk3obj
/// </summary>
public class StarShapeDefinition : IBeamShapeDefinition {
  // Pre-baked 5-pointed star with 10 points (alternating outer/inner radius)
  // Outer points at unit radius, inner points at ~0.382 radius (golden ratio)
  private static readonly (float x, float y)[] STAR_POINTS = [
    (0.000f, 1.000f),   // Top outer
    (0.293f, 0.345f),   // Top-right inner
    (0.951f, 0.309f),   // Right outer
    (0.475f, -0.154f),  // Right inner
    (0.588f, -0.809f),  // Bottom-right outer
    (0.000f, -0.500f),  // Bottom inner
    (-0.588f, -0.809f), // Bottom-left outer
    (-0.475f, -0.154f), // Left inner
    (-0.951f, 0.309f),  // Left outer
    (-0.293f, 0.345f)   // Top-left inner
  ];

  public BeamShapeType ShapeType => BeamShapeType.STAR;
  public IReadOnlyList<(float x, float y)> UnitPoints => STAR_POINTS;
  public bool IsClosed => true;
  public float DefaultRadius => 30f;
  public float DefaultWidth => 1.5f;
}