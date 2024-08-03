using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Mod.Zones;

namespace Jailbreak.Public.Mod.Trail;

public interface ITrailSegment {
  float GetTimeAlive() { return Server.CurrentTime - GetSpawnTime(); }
  float GetSpawnTime();

  Vector GetStart();
  Vector GetEnd();

  float GetDistanceSquared(Vector position) {
    return ConvexHullUtil.DistanceToSegmentSquared(position, GetStart(),
      GetEnd());
  }

  float GetDistance(Vector position) {
    return MathF.Sqrt(GetDistanceSquared(position));
  }

  void Remove();
}