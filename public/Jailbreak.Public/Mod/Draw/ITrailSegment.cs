using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Extensions;

namespace Jailbreak.Public.Mod.Draw;

public interface ITrailSegment {
  float GetTimeAlive();
  Vector GetStart();
  Vector GetEnd();

  double GetMinDistanceSquared(Vector position) {
    return Math.Min(GetStart().DistanceSquared(position),
      GetEnd().DistanceSquared(position));
  }

  double GetMinDistance(Vector position) {
    return Math.Sqrt(GetMinDistanceSquared(position));
  }

  double GetMaxDistanceSquared(Vector position) {
    return Math.Max(GetStart().DistanceSquared(position),
      GetEnd().DistanceSquared(position));
  }

  double GetMaxDistance(Vector position) {
    return Math.Sqrt(GetMaxDistanceSquared(position));
  }
}