using CS2DrawShared;
using Jailbreak.Public.Mod.Warden.Enums;

namespace Jailbreak.Public.Mod.Warden;

/// <summary>
/// IShapeSetup implementation that delegates to the correct setup
/// for each MarkerShapeType. Register once on plugin load, reuse per draw.
/// </summary>
public sealed class MarkerShapeSetup(MarkerShapeType type, float radius,
  int particles) : IShapeSetup {
  private readonly MarkerShapeType type = type;
  private readonly int particles = particles;

  public string EffectKey => type.ToEffectKey();

  public void Configure(IParticleConfigurator cp, int particleCount) {
    var count = calcCount(particleCount);
    cp.SetCp(2, count, count - 1, 0);
    cp.SetCp(5, radius, radius, 0);
  }

  /// <summary>
  /// Rounds particle count to a valid multiple for this shape's side count.
  /// Formula: (floor(n / sides) * sides) + 1
  /// Shapes with no fixed sides (Circle) pass through directly.
  /// </summary>
  private int calcCount(int requested)
    => type switch {
      MarkerShapeType.CIRCLE   => requested, // no sides — pass through
      MarkerShapeType.TRIANGLE => calcCountBySides(requested, 3),
      MarkerShapeType.DIAMOND  => calcCountBySides(requested, 4),
      MarkerShapeType.PENTAGON => calcCountBySides(requested, 5),
      MarkerShapeType.HEXAGON  => calcCountBySides(requested, 6),
      MarkerShapeType.STAR => calcCountBySides(requested,
        10), // 5 points × 2 edges
      MarkerShapeType.HEART      => calcCountBySides(requested, 2),
      MarkerShapeType.JELLY_BEAN => calcCountBySides(requested, 2),
      MarkerShapeType.AMONG_US   => calcCountBySides(requested, 4),
      _                          => requested
    };

  private static int calcCountBySides(int requestedCount, int sides) {
    var perSide = Math.Max(1, requestedCount / sides);
    return perSide * sides + 1;
  }
}