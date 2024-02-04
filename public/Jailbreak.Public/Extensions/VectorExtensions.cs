using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Extensions;

public static class VectorExtensions
{
    public static Vector Clone(this Vector vector)
    {
        var vec = new Vector
        {
            X = vector.X,
            Y = vector.Y,
            Z = vector.Z
        };
        return vec;
    }
    
    public static Vector Add(this Vector vector, Vector other)
    {
        vector.X += other.X;
        vector.Y += other.Y;
        vector.Z += other.Z;
        return vector;
    }

    public static Vector Scale(this Vector vector, float scale)
    {
        vector.X *= scale;
        vector.Y *= scale;
        vector.Z *= scale;
        return vector;
    }

    public static Vector Normalize(this Vector vector)
    {
        var length = vector.Length();
        vector.X /= length;
        vector.Y /= length;
        vector.Z /= length;
        return vector;
    }

    public static float Distance(this Vector vector, Vector other)
    {
        return (float)System.Math.Sqrt(vector.DistanceSquared(other));
    }

    public static float DistanceSquared(this Vector vector, Vector other)
    {
        return (float)(
            System.Math.Pow(vector.X - other.X, 2) +
            System.Math.Pow(vector.Y - other.Y, 2) +
            System.Math.Pow(vector.Z - other.Z, 2)
        );
    }
}