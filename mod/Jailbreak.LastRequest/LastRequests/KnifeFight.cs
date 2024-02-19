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
        if (@event.Userid.Slot != prisoner.Slot && @event.Userid.Slot != guard.Slot)
            return HookResult.Continue;

        if (@event.Userid.Slot == prisoner.Slot)
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

        plugin.AddTimer(3, Execute);
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