using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Extensions;

public static class VectorExtensions {
  public static Vector Clone(this Vector vector) {
    return new Vector(vector.X, vector.Y, vector.Z);
  }

  /// <summary>
  /// Calculates the (Euclidean) distance between the two vectors
  /// Where possible, use DistanceSquared instead for performance reasons
  /// </summary>
  /// <param name="vector"></param>
  /// <param name="other"></param>
  /// <returns></returns>
  public static float Distance(this Vector vector, Vector other) {
    return (float)Math.Sqrt(vector.DistanceSquared(other));
  }

  /// <summary>
  /// Calculates the squared (Euclidean) distance between the two vectors
  /// </summary>
  /// <param name="vector"></param>
  /// <param name="other"></param>
  /// <returns></returns>
  public static float DistanceSquared(this Vector vector, Vector other) {
    return MathF.Pow(vector.X - other.X, 2) + MathF.Pow(vector.Y - other.Y, 2)
      + MathF.Pow(vector.Z - other.Z, 2);
  }

  /// <summary>
  /// Calculates the horizontal distance between the two vectors
  /// Where possible, use HorizontalDistanceSquared instead for performance reasons
  /// </summary>
  /// <param name="vector"></param>
  /// <param name="other"></param>
  /// <returns></returns>
  public static float HorizontalDistance(this Vector vector, Vector other) {
    return MathF.Sqrt(vector.HorizontalDistanceSquared(other));
  }

  /// <summary>
  /// Calculates the squared horizontal distance between the two vectors
  /// </summary>
  /// <param name="vector"></param>
  /// <param name="other"></param>
  /// <returns></returns>
  public static float
    HorizontalDistanceSquared(this Vector vector, Vector other) {
    return MathF.Pow(vector.X - other.X, 2) + MathF.Pow(vector.Y - other.Y, 2);
  }
}