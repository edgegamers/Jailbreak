using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Extensions;

public static class VectorExtensions {
  public static Vector Clone(this Vector vector) {
    var vec = new Vector { X = vector.X, Y = vector.Y, Z = vector.Z };
    return vec;
  }

  public static float Distance(this Vector vector, Vector other) {
    return (float)Math.Sqrt(vector.DistanceSquared(other));
  }

  public static float DistanceSquared(this Vector vector, Vector other) {
    return (float)(Math.Pow(vector.X - other.X, 2)
      + Math.Pow(vector.Y - other.Y, 2) + Math.Pow(vector.Z - other.Z, 2));
  }
}