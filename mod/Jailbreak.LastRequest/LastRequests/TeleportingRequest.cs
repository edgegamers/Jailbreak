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
    State = LRState.PENDING;

    Guard.Teleport(Prisoner);

    Guard.Freeze();
    Prisoner.Freeze();
    Plugin.AddTimer(1, () => {
      Guard.UnFreeze();
      Prisoner.UnFreeze();
    });
  }
}