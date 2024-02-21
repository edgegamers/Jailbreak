using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.LastRequest.LastRequests;

/// <summary>
/// Represents a Last Request that involves PvP damage, meaning the two
/// involved players (and only those two) are allowed to deal damage to each other.
///
/// Automatically strips weapons, counts down, and calls Execute after 4 seconds.
/// </summary>
public abstract class PvPDamageRequest : AbstractLastRequest
{
    public PvPDamageRequest(BasePlugin plugin, CCSPlayerController prisoner, CCSPlayerController guard) : base(plugin,
        prisoner, guard)
    {
        plugin.RegisterEventHandler<EventPlayerHurt>(OnPlayerHurt);
    }

    public HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
    {
        bool hurtInLR = @event.Userid.Slot == prisoner.Slot || @event.Userid.Slot == guard.Slot;
        if(!hurtInLR) return HookResult.Continue;

        if (@event.Attacker == null)
            return HookResult.Continue;
        
        bool attackerInLR = @event.Attacker.Slot == prisoner.Slot || @event.Attacker.Slot == guard.Slot;
        if(attackerInLR) return HookResult.Continue;
        
        @event.DmgHealth = 0;
        return HookResult.Changed;
    }

    public override void Setup()
    {
        // Strip weapons, teleport T to CT
        prisoner.RemoveWeapons();
        guard.RemoveWeapons();
        guard.Teleport(prisoner.Pawn.Value!.AbsOrigin!, prisoner.Pawn.Value.AbsRotation!, new Vector());
        state = LRState.Pending;
        for (var i = 3; i >= 1; i--)
        {
            var copy = i;
            plugin.AddTimer(3 - i, () => { PrintToParticipants($"{copy}..."); });
        }

        plugin.AddTimer(4, Execute);
    }

    public override void End(LRResult result)
    {
        state = LRState.Completed;
    }
}