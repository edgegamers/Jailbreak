using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.Public.Mod.LastRequest;

public interface ILastRequestFactory : IPluginBehavior {
  AbstractLastRequest CreateLastRequest(CCSPlayerController prisoner,
    CCSPlayerController guard, LRType type);

  bool IsValidType(LRType type);
}