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
using Microsoft.VisualBasic;

namespace Jailbreak.LastGuard;

public class LastGuard(LastGuardConfig config,
  ILastGuardNotifications notifications, ILastRequestManager lrManager)
  : ILastGuardService, IPluginBehavior {
  private bool canStart;
  private bool isLastGuard = false;
  private List<CCSPlayerController> lastGuardPrisoners = new();
  private readonly Random rng = new();

  private static readonly HashSet<string> NON_WEAPONS = new() {
    "weapon_knife",
    "weapon_c4",
    "weapon_hegrenade",
    "weapon_flashbang",
    "weapon_smokegrenade",
    "weapon_decoy",
    "weapon_incgrenade",
    "weapon_healthshot",
    "weapon_molotov",
    "weapon_taser"
  };

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

    isLastGuard = true;

    var guardCalcHealth = CalculateHealth();

    guardPlayerPawn.Health = guardCalcHealth;
    Utilities.SetStateChanged(guardPlayerPawn, "CBaseEntity", "m_iHealth");

    // foreach (var player in Utilities.GetPlayers().Where(p => p.IsReal()))
    //     player.ExecuteClientCommand("play sounds/lastct");

    lastGuardPrisoners = Utilities.GetPlayers()
     .Where(p => p is { PawnIsAlive: true, Team: CsTeam.Terrorist })
     .ToList();

    var prisonerHp =
      lastGuardPrisoners.Sum(
        prisoner => prisoner.PlayerPawn?.Value?.Health ?? 0);

    notifications.LGStarted(guardCalcHealth, prisonerHp)
     .ToAllCenter()
     .ToAllChat();

    if (string.IsNullOrEmpty(config.LastGuardWeapon)) return;

    foreach (var player in lastGuardPrisoners)
      player.GiveNamedItem(config.LastGuardWeapon);
  }

  [GameEventHandler]
  public HookResult OnPlayerDeathEvent(EventPlayerDeath @event,
    GameEventInfo info) {
    var player = @event.Userid;
    if (player == null) return HookResult.Continue;
    checkLastGuard(@event.Userid);

    if (!isLastGuard) return HookResult.Continue;

    if (player.Team != CsTeam.Terrorist) return HookResult.Continue;

    giveGun(player);
    return HookResult.Continue;
  }

  [GameEventHandler]
  public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event,
    GameEventInfo info) {
    checkLastGuard(@event.Userid);
    return HookResult.Continue;
  }

  private void giveGun(CCSPlayerController poi) {
    lastGuardPrisoners = lastGuardPrisoners.Where(p
        => p is { IsValid: true, PawnIsAlive: true } && poi.SteamID != p.SteamID
        && !playerHasGun(p))
     .ToList();
    if (lastGuardPrisoners.Count == 0) { return; }

    var random = lastGuardPrisoners[rng.Next(lastGuardPrisoners.Count)];
    random.GiveNamedItem("weapon_glock");
    lastGuardPrisoners.Remove(random);
  }

  private bool playerHasGun(CCSPlayerController player) {
    var weapons = player.Pawn.Value?.WeaponServices;
    if (weapons == null) return false;
    foreach (var weapon in weapons.MyWeapons) {
      if (weapon == null || weapon.Value == null) continue;
      if (NON_WEAPONS.Contains(weapon.Value!.DesignerName)) continue;
      return true;
    }

    return false;
  }

  private void checkLastGuard(CCSPlayerController? poi) {
    if (poi == null) return;
    lastGuardPrisoners.Remove(poi);
    if (poi.Team != CsTeam.CounterTerrorist) return;
    var aliveCts = Utilities.GetPlayers()
     .Count(plr => plr.SteamID != poi.SteamID && plr is {
        PawnIsAlive: true, Team: CsTeam.CounterTerrorist
      });

    if (aliveCts != 1 || lrManager.IsLREnabled) return;
    var lastGuard = Utilities.GetPlayers()
     .First(plr => plr.IsReal() && plr != poi && plr is {
        PawnIsAlive: true, Team: CsTeam.CounterTerrorist
      });

    if (canStart) StartLastGuard(lastGuard);
  }

  [GameEventHandler]
  public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info) {
    isLastGuard = false;
    return HookResult.Continue;
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