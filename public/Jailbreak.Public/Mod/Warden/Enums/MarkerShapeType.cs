namespace Jailbreak.Public.Mod.Warden.Enums;

public enum MarkerShapeType {
  CIRCLE,
  TRIANGLE,
  SQUARE,
  DIAMOND,
  PENTAGON,
  HEXAGON,
  HEART,
  STAR,
  JELLY_BEAN,
  AMONG_US
}

/// <summary>
/// Extension methods for MarkerShape.
/// Converts enum values to CS2Draw effect keys and creates the correct IShapeSetup.
/// </summary>
public static class MarkerShapeTypeExtensions {
  /// <summary>
  /// Returns the CS2Draw effect key for this shape.
  /// Must match a key registered in cs2draw.json.
  /// </summary>
  public static string ToEffectKey(this MarkerShapeType shapeType)
    => shapeType switch {
      MarkerShapeType.CIRCLE     => "marker_circle",
      MarkerShapeType.TRIANGLE   => "marker_triangle",
      MarkerShapeType.SQUARE     => "marker_square",
      MarkerShapeType.DIAMOND    => "marker_diamond",
      MarkerShapeType.PENTAGON   => "marker_pentagon",
      MarkerShapeType.HEXAGON    => "marker_hexagon",
      MarkerShapeType.HEART      => "marker_heart",
      MarkerShapeType.STAR       => "marker_star",
      MarkerShapeType.JELLY_BEAN => "marker_jelly_bean",
      MarkerShapeType.AMONG_US   => "marker_among_us",
      _ => throw new ArgumentOutOfRangeException(nameof(shapeType), shapeType,
        null)
    };

  public static string ToDisplayName(this MarkerShapeType shapeType)
    => shapeType switch {
      MarkerShapeType.CIRCLE     => "Circle",
      MarkerShapeType.TRIANGLE   => "Triangle",
      MarkerShapeType.SQUARE     => "Square",
      MarkerShapeType.DIAMOND    => "Diamond",
      MarkerShapeType.PENTAGON   => "Pentagon",
      MarkerShapeType.HEXAGON    => "Hexagon",
      MarkerShapeType.HEART      => "Heart",
      MarkerShapeType.STAR       => "Star",
      MarkerShapeType.JELLY_BEAN => "Jelly Bean",
      MarkerShapeType.AMONG_US   => "Among Us",
      _                          => shapeType.ToString()
    };

  /// <summary>
  /// Returns all marker shapes.
  /// </summary>
  public static IReadOnlyList<MarkerShapeType> All()
    => Enum.GetValues<MarkerShapeType>().ToList();
}