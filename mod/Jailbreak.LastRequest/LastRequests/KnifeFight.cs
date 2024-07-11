using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.LastRequest.LastRequests;

public class KnifeFight(BasePlugin plugin, ILastRequestManager manager,
  CCSPlayerController prisoner, CCSPlayerController guard)
  : WeaponizedRequest(plugin, manager, prisoner, guard) {
  public override LRType Type => LRType.KNIFE_FIGHT;

  public override void Execute() {
    PrintToParticipants("Go!");
    Prisoner.GiveNamedItem("weapon_knife");
    Guard.GiveNamedItem("weapon_knife");
    State = LRState.ACTIVE;
  }

  public override void OnEnd(LRResult result) { State = LRState.COMPLETED; }
}