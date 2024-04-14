using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.LastRequest.LastRequests;

/// <summary>
/// Represents a Last Request that involves direct PvP combat.
///
/// Automatically strips weapons, counts down, and calls Execute after 4 seconds.
/// </summary>
public abstract class WeaponizedRequest(
    BasePlugin plugin,
    ILastRequestManager manager,
    CCSPlayerController prisoner,
    CCSPlayerController guard)
    : TeleportingRequest(plugin, manager, prisoner, guard)
{
    public override void Setup()
    {
        base.Setup();

        // Strip weapons, teleport T to CT
        prisoner.RemoveWeapons();
        guard.RemoveWeapons();
        for (var i = 3; i >= 1; i--)
        {
            var copy = i;
            plugin.AddTimer(3 - i, () => { PrintToParticipants($"{copy}..."); });
        }

        plugin.AddTimer(3, Execute);
    }

    public override void OnEnd(LRResult result)
    {
        switch (result)
        {
            case LRResult.GuardWin:
                prisoner.Pawn.Value?.CommitSuicide(false, true);
                break;
            case LRResult.PrisonerWin:
                guard.Pawn.Value?.CommitSuicide(false, true);
                break;
        }

        state = LRState.Completed;
    }
}