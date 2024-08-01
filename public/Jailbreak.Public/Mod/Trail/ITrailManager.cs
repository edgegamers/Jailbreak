using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Behaviors;

namespace Jailbreak.Public.Mod.Trail;

public interface ITrailManager<T> : IPluginBehavior where T : ITrailSegment {
  bool AddPlayerTrail(CCSPlayerController player);
  bool HasPlayerTrail(CCSPlayerController player);
  bool RemovePlayerTrail(CCSPlayerController player);
  T? GetPlayerTrail(CCSPlayerController player);
}