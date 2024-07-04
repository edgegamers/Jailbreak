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

public class LastGuard : ILastGuardService, IPluginBehavior
{

    private readonly LastGuardConfig _config;
    private readonly ILastGuardNotifications _notifications;
    private bool _canStart = false;
    
    public LastGuard(LastGuardConfig config, ILastGuardNotifications notifications)
    {
        _config = config;
        _notifications = notifications;
    }   

    public void Start(BasePlugin plugin)
    {
        plugin.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeathEvent);
        plugin.RegisterEventHandler<EventRoundStart>(OnRoundStartEvent);
    }

    [GameEventHandler]
    public HookResult OnPlayerDeathEvent(EventPlayerDeath @event, GameEventInfo info)
    {
        if (Utilities.GetPlayers().Count(plr => plr.IsReal() && plr is { PawnIsAlive: true, Team: CsTeam.CounterTerrorist}) == 1)
        {
            StartLastGuard(Utilities.GetPlayers().First(plr => plr.IsReal() && plr is { PawnIsAlive: true, Team: CsTeam.CounterTerrorist }));
        }
        
        return HookResult.Continue;
    }
    
    [GameEventHandler]
    public HookResult OnRoundStartEvent(EventRoundStart @event, GameEventInfo info)
    {
        _canStart = Utilities.GetPlayers().Count(plr => plr.IsReal() && plr is { PawnIsAlive: true, Team: CsTeam.CounterTerrorist}) >= _config.MinimumCTs;
        return HookResult.Continue;
    }
    
    
    
    public int CalculateHealth()
    {
        var aliveTerrorists = Utilities.GetPlayers()
            .Where(plr => plr.IsReal() && plr is { PawnIsAlive: true, Team: CsTeam.Terrorist });

        foreach (var terrorist in aliveTerrorists)
        {
            if (terrorist.Health > 100) terrorist.Health = 100;
        }
        
        return aliveTerrorists.Select(player => player.Health).Select(playerHealth => playerHealth >= 100 ? 45 : (int)(playerHealth * 0.45)).Sum();
    }

    public void StartLastGuard(CCSPlayerController lastGuard)
    {
        if (!_canStart) return;
        lastGuard.Health = CalculateHealth();

        var aliveTerrorists = Utilities.GetPlayers()
            .Where(plr => plr.IsReal() && plr is { PawnIsAlive: true, Team: CsTeam.Terrorist });

        
        foreach (var player in aliveTerrorists)
        {
            player.GiveNamedItem(_config.LastGuardWeapon);
        }

        _notifications.LG_STARTED.ToAllCenter().ToAllChat();
    }
}