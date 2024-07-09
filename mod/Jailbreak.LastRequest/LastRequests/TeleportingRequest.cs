using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.LastRequest.LastRequests;

public abstract class TeleportingRequest(BasePlugin plugin,
  ILastRequestManager manager, CCSPlayerController prisoner,
  CCSPlayerController guard)
  : AbstractLastRequest(plugin, manager, prisoner, guard) {
  public override void Setup() {
    state = LRState.Pending;

    guard.Teleport(prisoner);

    guard.Freeze();
    prisoner.Freeze();
    plugin.AddTimer(1, () => {
      guard.UnFreeze();
      prisoner.UnFreeze();
    });
  }
}