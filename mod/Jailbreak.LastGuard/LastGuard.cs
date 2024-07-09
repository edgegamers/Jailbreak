using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastGuard;
using Jailbreak.Public.Mod.LastRequest;

namespace Jailbreak.LastGuard;

public class LastGuard(LastGuardConfig config, ILastGuardNotifications notifications, ILastRequestManager lrManager)
    : ILastGuardService, IPluginBehavior
{
    private bool _canStart;

    [GameEventHandler]
    public HookResult OnPlayerDeathEvent(EventPlayerDeath @event, GameEventInfo info)
    {
        checkLastGuard(@event.Userid);
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        checkLastGuard(@event.Userid);
        return HookResult.Continue;
    }

    private void checkLastGuard(CCSPlayerController? poi)
    {
        if (poi == null) return;
        if (poi.Team != CsTeam.CounterTerrorist) ;
        var aliveCts = Utilities.GetPlayers()
            .Count(plr => plr.IsReal() && plr is { PawnIsAlive: true, Team: CsTeam.CounterTerrorist }) - 1;

        if (aliveCts != 1 || lrManager.IsLREnabled) ;
        var lastGuard = Utilities.GetPlayers().First(plr =>
            plr.IsReal() && plr != poi && plr is { PawnIsAlive: true, Team: CsTeam.CounterTerrorist });

        if (_canStart)
            StartLastGuard(lastGuard);
    }

    [GameEventHandler]
    public HookResult OnRoundStartEvent(EventRoundStart @event, GameEventInfo info)
    {
        _canStart = Utilities.GetPlayers()
                        .Count(plr => plr.IsReal() && plr is { PawnIsAlive: true, Team: CsTeam.CounterTerrorist }) >=
                    config.MinimumCTs;
        return HookResult.Continue;
    }

    public int CalculateHealth()
    {
        var aliveTerrorists = Utilities.GetPlayers()
            .Where(plr => plr.IsReal() && plr is { PawnIsAlive: true, Team: CsTeam.Terrorist }).ToList();

        return aliveTerrorists.Select(player => player.PlayerPawn?.Value?.Health ?? 0)
            .Select(playerHealth => (int)(Math.Min(playerHealth, 200) * 0.8)).Sum();
    }

    public void StartLastGuard(CCSPlayerController lastGuard)
    {
        var ctPlayerPawn = lastGuard.PlayerPawn.Value;

        if (ctPlayerPawn == null || !ctPlayerPawn.IsValid) return;

        var ctHealth = ctPlayerPawn.Health;
        var ctCalcHealth = CalculateHealth();

        ctPlayerPawn.Health = ctHealth > ctCalcHealth ? 125 : ctCalcHealth;
        Utilities.SetStateChanged(ctPlayerPawn, "CBaseEntity", "m_iHealth");

        foreach (var player in Utilities.GetPlayers().Where(p => p.IsReal()))
            player.ExecuteClientCommand("play sounds/lastct");

        var aliveTerrorists = Utilities.GetPlayers()
            .Where(p => p.IsReal() && p is { PawnIsAlive: true, Team: CsTeam.Terrorist }).ToList();

        var guardHp = lastGuard.PlayerPawn?.Value?.Health ?? 0;
        var prisonerHp = aliveTerrorists.Sum(prisoner => prisoner.PlayerPawn?.Value?.Health ?? 0);

        notifications.LG_STARTED(guardHp, prisonerHp).ToAllCenter().ToAllChat();

        if (string.IsNullOrEmpty(config.LastGuardWeapon)) return;

        foreach (var player in aliveTerrorists)
        {
            player.GiveNamedItem(config.LastGuardWeapon);
        }
    }
}