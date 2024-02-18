using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.LastRequest.LastRequests;

public class KnifeFight : AbstractLastRequest
{
    public KnifeFight(BasePlugin plugin, CCSPlayerController prisoner, CCSPlayerController guard) : base(plugin,
        prisoner, guard)
    {
        plugin.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
    }

    public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        if (@event.Userid != prisoner && @event.Userid != guard)
            return HookResult.Continue;

        if (@event.Userid == prisoner)
            End(LRResult.GuardWin);
        else
            End(LRResult.PrisonerWin);
        return HookResult.Continue;
    }

    public override void Setup()
    {
        // Strip weapons, teleport T to CT
        prisoner.RemoveWeapons();
        guard.RemoveWeapons();
        this.state = LRState.Pending;
    }

    public override void Execute()
    {
        prisoner.GiveNamedItem("weapon_knife");
        guard.GiveNamedItem("weapon_knife");
        this.state = LRState.Active;
    }

    public override void End(LRResult result)
    {
        this.state = LRState.Completed;
    }
}