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

public class LastGuard(LastGuardConfig config,
  ILastGuardNotifications notifications, ILastRequestManager lrManager)
  : ILastGuardService, IPluginBehavior {
  private bool canStart;

  public int CalculateHealth() {
    var aliveTerrorists = Utilities.GetPlayers()
     .Where(plr
        => plr.IsReal() && plr is { PawnIsAlive: true, Team: CsTeam.Terrorist })
     .ToList();

    return aliveTerrorists
     .Select(player => player.PlayerPawn?.Value?.Health ?? 0)
     .Select(playerHealth => (int)(Math.Min(playerHealth, 200) * 0.8))
     .Sum();
  }

  public void StartLastGuard(CCSPlayerController lastGuard) {
    var guardPlayerPawn = lastGuard.PlayerPawn.Value;

    if (guardPlayerPawn == null || !guardPlayerPawn.IsValid) return;

    var guardCalcHealth = CalculateHealth();

    guardPlayerPawn.Health = guardCalcHealth;
    Utilities.SetStateChanged(guardPlayerPawn, "CBaseEntity", "m_iHealth");

    // foreach (var player in Utilities.GetPlayers().Where(p => p.IsReal()))
    //     player.ExecuteClientCommand("play sounds/lastct");

    var aliveTerrorists = Utilities.GetPlayers()
     .Where(p
        => p.IsReal() && p is { PawnIsAlive: true, Team: CsTeam.Terrorist })
     .ToList();

    var prisonerHp =
      aliveTerrorists.Sum(prisoner => prisoner.PlayerPawn?.Value?.Health ?? 0);

    notifications.LGStarted(guardCalcHealth, prisonerHp)
     .ToAllCenter()
     .ToAllChat();

    if (string.IsNullOrEmpty(config.LastGuardWeapon)) return;

    foreach (var player in aliveTerrorists)
      player.GiveNamedItem(config.LastGuardWeapon);
  }

  [GameEventHandler]
  public HookResult OnPlayerDeathEvent(EventPlayerDeath @event,
    GameEventInfo info) {
    checkLastGuard(@event.Userid);
    return HookResult.Continue;
  }

  [GameEventHandler]
  public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event,
    GameEventInfo info) {
    checkLastGuard(@event.Userid);
    return HookResult.Continue;
  }

  private void checkLastGuard(CCSPlayerController? poi) {
    if (poi == null) return;
    if (poi.Team != CsTeam.CounterTerrorist) return;
    var aliveCts = Utilities.GetPlayers()
     .Count(plr
        => plr.IsReal() && plr is {
          PawnIsAlive: true, Team: CsTeam.CounterTerrorist
        }) - 1;

    if (aliveCts != 1 || lrManager.IsLREnabled) return;
    var lastGuard = Utilities.GetPlayers()
     .First(plr => plr.IsReal() && plr != poi && plr is {
        PawnIsAlive: true, Team: CsTeam.CounterTerrorist
      });

    if (canStart) StartLastGuard(lastGuard);
  }

  [GameEventHandler]
  public HookResult OnRoundStartEvent(EventRoundStart @event,
    GameEventInfo info) {
    canStart = Utilities.GetPlayers()
     .Count(plr
        => plr.IsReal() && plr is {
          PawnIsAlive: true, Team: CsTeam.CounterTerrorist
        }) >= config.MinimumCTs;
    return HookResult.Continue;
  }
}