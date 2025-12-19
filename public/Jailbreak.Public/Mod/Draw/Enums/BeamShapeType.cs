namespace Jailbreak.Public.Mod.Draw.Enums;

/// <summary>
///   Represents the type of beam, shape that can be drawn in 3D space using beams.
/// </summary>
public enum BeamShapeType {
  CIRCLE,
  SQUARE,
  DIAMOND,
  STAR,
  TRIANGLE,
  PENTAGON,
  HEXAGON,
  HEART,
  AMONG_US
}

public static class BeamShapeTypeExtensions {
  public static string ToFriendlyString(this BeamShapeType shapeType) {
    return shapeType switch {
      BeamShapeType.CIRCLE   => "Circle",
      BeamShapeType.SQUARE   => "Square",
      BeamShapeType.DIAMOND  => "Diamond",
      BeamShapeType.STAR     => "Star",
      BeamShapeType.TRIANGLE => "Triangle",
      BeamShapeType.PENTAGON => "Pentagon",
      BeamShapeType.HEXAGON  => "Hexagon",
      BeamShapeType.HEART    => "Heart",
      BeamShapeType.AMONG_US => "Among Us",
      _                      => "Unknown"
    };
  }
}