using Jailbreak.Public.Mod.Draw.Enums;

namespace Jailbreak.Public.Mod.Draw;

/// <summary>
///   Defines a 2D shape in unit space (roughly normalized to [-1, 1]).
///   Each shape is represented as a polyline with 2D points.
/// </summary>
public interface IBeamShapeDefinition {
  /// <summary>
  ///   The type of beam, shape this definition represents.
  /// </summary>
  BeamShapeType ShapeType { get; }

  /// <summary>
  ///   The unit 2D points that define the shape in local space.
  ///   These points should be roughly normalized to [-1, 1] range.
  /// </summary>
  IReadOnlyList<(float x, float y)> UnitPoints { get; }

  /// <summary>
  ///   Whether the shape is closed (the last point connects back to the first).
  /// </summary>
  bool IsClosed { get; }

  /// <summary>
  ///   Default radius/scale for this shape.
  /// </summary>
  float DefaultRadius { get; }

  /// <summary>
  ///   Default line width for rendering this shape.
  /// </summary>
  float DefaultWidth { get; }
}