using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.Rebel;

namespace Jailbreak.Rebel;

public class RebelListener(IRebelService rebelService, ILastRequestManager lastRequestManager)
    : IPluginBehavior
{
    [GameEventHandler]
    public HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!player.IsReal())
            return HookResult.Continue;
        if (player.GetTeam() != CsTeam.CounterTerrorist)
            return HookResult.Continue;

        var attacker = @event.Attacker;
        if (!attacker.IsReal())
            return HookResult.Continue;

        if (attacker.GetTeam() != CsTeam.Terrorist)
            return HookResult.Continue;

        if (lastRequestManager.IsInLR(attacker))
            return HookResult.Continue;

        rebelService.MarkRebel(attacker);
        return HookResult.Continue;
    }
}