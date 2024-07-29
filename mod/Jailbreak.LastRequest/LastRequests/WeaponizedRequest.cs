using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.LastRequest.LastRequests;

/// <summary>
///   Represents a Last Request that involves direct PvP combat.
///   Automatically strips weapons, counts down, and calls Execute after 4 seconds.
/// </summary>
public abstract class WeaponizedRequest(BasePlugin plugin,
  ILastRequestManager manager, CCSPlayerController prisoner,
  CCSPlayerController guard)
  : TeleportingRequest(plugin, manager, prisoner, guard) {
  public override void Setup() {
    base.Setup();

    Prisoner.RemoveWeapons();
    Guard.RemoveWeapons();
    for (var i = 3; i >= 1; i--) {
      var copy = i;
      Plugin.AddTimer(3 - i, () => { PrintToParticipants($"{copy}..."); });
    }

    Plugin.AddTimer(3, () => {
      if (State != LRState.PENDING) return;
      Execute();
    });
  }

  public override void OnEnd(LRResult result) {
    switch (result) {
      case LRResult.GUARD_WIN:
        Prisoner.Pawn.Value?.CommitSuicide(false, true);
        break;
      case LRResult.PRISONER_WIN:
        Guard.Pawn.Value?.CommitSuicide(false, true);
        break;
    }

    State = LRState.COMPLETED;
  }
}