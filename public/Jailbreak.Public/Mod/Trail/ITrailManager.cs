using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Behaviors;

namespace Jailbreak.Public.Mod.Trail;

public interface ITrailManager : IPluginBehavior {
  bool AddPlayerTrail(CCSPlayerController player);
  bool HasPlayerTrail(CCSPlayerController player);
  bool RemovePlayerTrail(CCSPlayerController player);
  AbstractTrail? GetPlayerTrail(CCSPlayerController player);
}