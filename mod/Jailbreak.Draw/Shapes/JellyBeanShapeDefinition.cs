using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Draw.Enums;

namespace Jailbreak.Draw.Shapes;

/// <summary>
///   Jellybean (pill/capsule) shape definition with rounded ends.
///   Consists of two semicircles connected by straight sides.
///   Desmos visualization: https://www.desmos.com/calculator/jpvpg5uqsh
/// </summary>
public class JellyBeanShapeDefinition : IBeamShapeDefinition {
  // Pre-baked jellybean with elongated shape (unit radius for rounded ends)
  // Top semicircle, right side, bottom semicircle, left side
  private static readonly (float x, float y)[] JELLY_BEAN_POINTS = new[]
  {
    (0.550f,  0.100f),
    (0.720f,  0.240f),
    (0.820f,  0.420f),
    (0.850f,  0.600f),
    (0.800f,  0.750f),
    (0.650f,  0.850f),
    (0.420f,  0.900f),
    (0.180f,  0.880f),
    (-0.050f, 0.800f),
    (-0.200f, 0.650f),
    (-0.300f, 0.450f),
    (-0.340f, 0.200f),
    (-0.320f, -0.050f),
    (-0.220f, -0.300f),
    (-0.050f, -0.550f),
    (0.200f,  -0.750f),
    (0.450f,  -0.850f),
    (0.650f,  -0.820f),
    (0.780f,  -0.650f),
    (0.820f,  -0.400f),
    (0.780f,  -0.200f),
    (0.650f,  -0.050f),
  };

  public BeamShapeType ShapeType => BeamShapeType.JELLY_BEAN;
  public IReadOnlyList<(float x, float y)> UnitPoints => JELLY_BEAN_POINTS;
  public bool IsClosed => true;
  public float DefaultRadius => 35f;
  public float DefaultWidth => 1.5f;
}