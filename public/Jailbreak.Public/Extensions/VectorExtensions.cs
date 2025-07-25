using System.Numerics;
using CounterStrikeSharp.API.Core;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace Jailbreak.Public.Extensions;

public static class VectorExtensions {
  public static Vector Clone(this Vector vector) {
    return new Vector(vector.X, vector.Y, vector.Z);
  }

  /// <summary>
  ///   Calculates the (Euclidean) distance between the two vectors
  ///   Where possible, use DistanceSquared instead for performance reasons
  /// </summary>
  /// <param name="vector"></param>
  /// <param name="other"></param>
  /// <returns></returns>
  public static float Distance(this Vector? vector, Vector other) {
    return (float)Math.Sqrt(vector.DistanceSquared(other));
  }

  /// <summary>
  ///   Calculates the squared (Euclidean) distance between the two vectors
  /// </summary>
  /// <param name="vector"></param>
  /// <param name="other"></param>
  /// <returns></returns>
  public static float DistanceSquared(this Vector? vector, Vector other) {
    return MathF.Pow(vector.X - other.X, 2) + MathF.Pow(vector.Y - other.Y, 2)
      + MathF.Pow(vector.Z - other.Z, 2);
  }

  /// <summary>
  ///   Calculates the horizontal distance between the two vectors
  ///   Where possible, use HorizontalDistanceSquared instead for performance reasons
  /// </summary>
  /// <param name="vector"></param>
  /// <param name="other"></param>
  /// <returns></returns>
  public static float HorizontalDistance(this Vector vector, Vector other) {
    return MathF.Sqrt(vector.HorizontalDistanceSquared(other));
  }

  /// <summary>
  ///   Calculates the squared horizontal distance between the two vectors
  /// </summary>
  /// <param name="vector"></param>
  /// <param name="other"></param>
  /// <returns></returns>
  public static float
    HorizontalDistanceSquared(this Vector vector, Vector other) {
    return MathF.Pow(vector.X - other.X, 2) + MathF.Pow(vector.Y - other.Y, 2);
  }

  /// <summary>
  ///   Converts a CounterStrikeSharp Vector Into a Vector3 Class
  /// </summary>
  /// <param name="vector"></param>
  /// <returns></returns>
  public static Vector3 Into(this Vector vector) {
    return new Vector3(vector.X, vector.Y, vector.Z);
  }

  /// <summary>
  ///   Converts the given angle vector (pitch, yaw, roll) into directional unit vectors:
  ///   forward, right, and up.
  ///   Useful for translating eye angles or view angles into world-space directions.
  ///   Wraps the native `AngleVectors` call from the engine.
  /// </summary>
  /// <param name="input"></param>
  /// <param name="forward"></param>
  /// <param name="right"></param>
  /// <param name="up"></param>
  public static void AngleVectors(this Vector3 input, out Vector3 forward,
    out Vector3 right, out Vector3 up) {
    Vector3 tmpForward, tmpRight, tmpUp;

    unsafe {
      NativeAPI.AngleVectors((nint)(&input), (nint)(&tmpForward),
        (nint)(&tmpRight), (nint)(&tmpUp));
    }

    forward = tmpForward;
    right   = tmpRight;
    up      = tmpUp;
  }
}