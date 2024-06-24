using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastGuard;

namespace Jailbreak.LastGuard;

public class LastGuard : IPluginBehavior, ILastGuardService
{
    private BasePlugin plugin;
    private ILastGuardMessages messages;
    private bool lastGuard = false;

    public void Start(BasePlugin parent, ILastGuardMessages messages)
    {
        plugin = parent;
        this.messages = messages;
    }

    [GameEventHandler]
    public HookResult OnPlayerKilled(EventPlayerDeath @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null || !player.IsValid || !player.IsReal())
            return HookResult.Continue;
        if (player.Team != CsTeam.CounterTerrorist)
            return HookResult.Continue;
        var ctsAlive = Utilities.GetPlayers()
            .Where(p => p.IsReal() && p is { Team: CsTeam.CounterTerrorist, PawnIsAlive: true }).ToList();
        if (ctsAlive.Count == 2)
        {
            ctsAlive.Remove(player);
            StartLastGuard(ctsAlive[0]);
        }

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        
    }

    public bool IsLastGuard()
    {
        return lastGuard;
    }

    public void StartLastGuard(CCSPlayerController guard)
    {
        lastGuard = true;
        var totalHp = Utilities.GetPlayers()
            .Where(p => p.IsReal() && p is { PawnIsAlive: true, Team: CsTeam.Terrorist })
            .Sum(p => p.Health);

        if (guard.PlayerPawn.Value != null)
        {
            guard.PlayerPawn.Value.Health = totalHp;
            Utilities.SetStateChanged(guard.PlayerPawn.Value, "CBaseEntity", "m_iHealth");
        }

        messages.LastGuardActivated(guard, totalHp);
    }

    public void EndLastGuard()
    {
        lastGuard = false;
    }
}