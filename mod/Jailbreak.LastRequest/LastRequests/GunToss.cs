using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.LastRequest.LastRequests;

public class GunToss(BasePlugin plugin, ILastRequestManager manager,
  CCSPlayerController prisoner, CCSPlayerController guard)
  : TeleportingRequest(plugin, manager, prisoner, guard) {
  public override LRType Type => LRType.GUN_TOSS;

  public override void Setup() {
    base.Setup();

    Plugin.AddTimer(3, Execute);
  }

  public override void Execute() {
    Prisoner.GiveNamedItem("weapon_knife");
    Guard.GiveNamedItem("weapon_knife");
    Prisoner.GiveNamedItem("weapon_deagle");
    Guard.GiveNamedItem("weapon_deagle");
    State = LRState.ACTIVE;
  }

  public override void OnEnd(LRResult result) { State = LRState.COMPLETED; }
}