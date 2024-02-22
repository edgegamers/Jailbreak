using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.LastRequest.LastRequests;

/// <summary>
/// Represents a Last Request that involves direct PvP combat.
///
/// Automatically strips weapons, counts down, and calls Execute after 4 seconds.
/// </summary>
public abstract class WeaponizedRequest : AbstractLastRequest
{
    public WeaponizedRequest(BasePlugin plugin, ILastRequestManager manager, CCSPlayerController prisoner,
        CCSPlayerController guard) : base(plugin, manager, prisoner, guard)
    {
    }

    public override void Setup()
    {
        state = LRState.Pending;
        
        // Strip weapons, teleport T to CT
        prisoner.RemoveWeapons();
        guard.RemoveWeapons();
        guard.Teleport(prisoner.Pawn.Value!.AbsOrigin!, prisoner.Pawn.Value.AbsRotation!, new Vector());
        for (var i = 3; i >= 1; i--)
        {
            var copy = i;
            plugin.AddTimer(3 - i, () => { PrintToParticipants($"{copy}..."); });
        }

        plugin.AddTimer(4, Execute);
    }

    public override void OnEnd(LRResult result)
    {
        state = LRState.Completed;
    }
}