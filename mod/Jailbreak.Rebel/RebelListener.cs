using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Rebel;

namespace Jailbreak.Rebel;

public class RebelListener : IPluginBehavior
{
    private readonly IRebelService _rebelService;

    public RebelListener(IRebelService rebelService)
    {
        _rebelService = rebelService;
    }

    public void Start(BasePlugin parent)
    {
        parent.RegisterEventHandler<EventPlayerHurt>(OnPlayerHurt);
    }

    private HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
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

        _rebelService.MarkRebel(attacker);
        return HookResult.Continue;
    }
}