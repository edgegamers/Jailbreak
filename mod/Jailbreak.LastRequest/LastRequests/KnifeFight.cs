using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.LastRequest.LastRequests;

public class KnifeFight(
    BasePlugin plugin,
    ILastRequestManager manager,
    CCSPlayerController prisoner,
    CCSPlayerController guard)
    : WeaponizedRequest(plugin, manager,
        prisoner, guard)
{
    public override LRType type => LRType.KnifeFight;

    public override void Execute()
    {
        PrintToParticipants("Go!");
        prisoner.GiveNamedItem("weapon_knife");
        guard.GiveNamedItem("weapon_knife");
        state = LRState.Active;
    }

    public override void OnEnd(LRResult result)
    {
        state = LRState.Completed;
    }
}