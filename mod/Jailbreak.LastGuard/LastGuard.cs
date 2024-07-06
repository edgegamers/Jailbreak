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
        
    }

    [GameEventHandler]
    public HookResult OnPlayerDeathEvent(EventPlayerDeath @event, GameEventInfo info)
    {
        var target = @event.Userid;
        if (target == null) return HookResult.Continue;
        if (target.Team != CsTeam.CounterTerrorist) return HookResult.Continue;
        var aliveCts = Utilities.GetPlayers()
            .Count(plr => plr.IsReal() && plr is { PawnIsAlive: true, Team: CsTeam.CounterTerrorist }) - 1;
        
        Server.PrintToChatAll("Alive CTs: " + aliveCts);

        if (aliveCts == 1)
        {
            var lastGuard = Utilities.GetPlayers().First(plr => plr.IsReal() && plr != target && plr is { PawnIsAlive: true, Team: CsTeam.CounterTerrorist });
            
            StartLastGuard(lastGuard);
        }
        return HookResult.Continue;
    }
    
    [GameEventHandler]
    public HookResult OnRoundStartEvent(EventRoundStart @event, GameEventInfo info)
    {
        _canStart = Utilities.GetPlayers().Count(plr => plr.IsReal() && plr is { PawnIsAlive: true, Team: CsTeam.CounterTerrorist}) >= _config.MinimumCTs;
        Server.PrintToChatAll("Can start: " + _canStart);
        return HookResult.Continue;
    }
    
    public int CalculateHealth()
    {
        var aliveTerrorists = Utilities.GetPlayers()
            .Where(plr => plr.IsReal() && plr is { PawnIsAlive: true, Team: CsTeam.Terrorist });

        foreach (var terrorist in aliveTerrorists)
        {
            if (terrorist.PlayerPawn.Value?.Health > 100) terrorist.PlayerPawn.Value.Health = 100;
        }
        
        return aliveTerrorists.Select(player => player.PlayerPawn.Value.Health).Select(playerHealth => (int)(playerHealth * 0.45)).Sum();
    }

    public void StartLastGuard(CCSPlayerController lastGuard)
    {
        if (!_canStart) return;

        var ctPlayerPawn = lastGuard.PlayerPawn.Value;

        if (!ctPlayerPawn.IsValid) return;

        var ctHealth = ctPlayerPawn.Health;
        var ctCalcHealth = CalculateHealth();
        
        ctPlayerPawn.Health = ctHealth > ctCalcHealth ? 125 : ctCalcHealth;

        Utilities.SetStateChanged(ctPlayerPawn, "CBaseEntity", "m_iHealth");
        
        var aliveTerrorists = Utilities.GetPlayers()
            .Where(plr => plr.IsReal() && plr is { PawnIsAlive: true, Team: CsTeam.Terrorist });

        
        _notifications.LG_STARTED(lastGuard.PlayerPawn.Value.Health, aliveTerrorists.Select(plr => plr.PlayerPawn.Value.Health).Sum()).ToAllCenter().ToAllChat();

        if (string.IsNullOrEmpty(_config.LastGuardWeapon)) return;
        
        foreach (var player in aliveTerrorists)
        {
            player.GiveNamedItem(_config.LastGuardWeapon);
        }

    }
}
