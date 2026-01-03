using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Draw.Enums;

namespace Jailbreak.Draw.Shapes;

/// <summary>
///   Among Us crewmate shape definition (simplified silhouette).
///   Desmos visualization: https://www.desmos.com/calculator/q97gzbbkog
/// </summary>
public class AmongUsShapeDefinition : IBeamShapeDefinition {
  // Pre-baked Among Us crewmate silhouette with ~24 points
  // Simplified representation of the iconic shape
  private static readonly (float x, float y)[] AMONG_US_POINTS = [
    // Visor
    (0.620f, 0.290f),
    (0.720f, 0.420f),
    (0.560f, 0.580f),
    (0.320f, 0.620f),
    (0.080f, 0.580f),
    (-0.080f, 0.420f),
    (0.080f, 0.260f),
    (0.320f, 0.220f),
    (0.620f, 0.290f),
    
    //Leg & Front Torso
    (0.620f, -0.480f),
    (0.600f, -0.700f),
    (0.544f, -0.900f),
    (0.460f, -0.933f),
    (0.320f, -0.933f),
    (0.255f, -0.900f),
    (0.200f, -0.700f),
    (-0.160f, -0.700f),
    (-0.203f, -0.900f),
    (-0.290f, -0.933f),
    (-0.430f, -0.933f),
    (-0.492f, -0.900f),
    (-0.560f, -0.700f),
    
    //BackPack
    (-0.580f, -0.480f),
    (-0.780f, -0.480f),
    (-0.780f, 0.320f),
    (-0.580f, 0.320f),
    
    //Head
    (-0.500f, 0.620f),
    (-0.300f, 0.827f),
    (-0.100f, 0.907f),
    (0.144f, 0.907f),
    (0.341f, 0.827f),
    (0.560f, 0.580f),
  ];

  public BeamShapeType ShapeType => BeamShapeType.AMONG_US;
  public IReadOnlyList<(float x, float y)> UnitPoints => AMONG_US_POINTS;
  public bool IsClosed => true;
  public float DefaultRadius => 40f; // Slightly larger default for this complex shape
  public float DefaultWidth => 1.5f;
}