using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.Rebel;

namespace Jailbreak.Rebel;

public class RebelListener : IPluginBehavior
{
    private readonly IRebelService _rebelService;
    private readonly ILastRequestManager _lastRequestManager;

    public RebelListener(IRebelService rebelService, ILastRequestManager lastRequestManager)
    {
        _rebelService = rebelService;
        _lastRequestManager = lastRequestManager;
    }

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

        if (_lastRequestManager.IsInLR(attacker))
            return HookResult.Continue;
        
        _rebelService.MarkRebel(attacker);
        return HookResult.Continue;
    }
}