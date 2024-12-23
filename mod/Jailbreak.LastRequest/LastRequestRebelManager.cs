using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Menu;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.Rebel;
using Jailbreak.Validator;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views.LastRequest;
using Jailbreak.Public.Extensions;

namespace Jailbreak.LastRequest;

public class LastRequestRebelManager(IRebelService rebelService,
  ILRLocale messages) : ILastRequestRebelManager {
  public static readonly FakeConVar<string> CV_REBEL_WEAPON =
    new("css_jb_rebel_t_weapon", "Weapon to give to rebeller during LR",
      "weapon_m249", ConVarFlags.FCVAR_NONE, new ItemValidator());

  public static readonly FakeConVar<bool> CV_REBEL_ON = new("css_jb_rebel_on",
    "If true, rebelling will be enabled during LR", true);

  public static readonly FakeConVar<int> CV_MAX_CT_HEALTH_CONTRIBUTION = new(
    "css_jb_rebel_ct_max_hp", "Max HP to contribute per CT to rebeller", 200,
    ConVarFlags.FCVAR_NONE, new RangeValidator<int>(1, 1000));

  public static readonly FakeConVar<double> CV_T_HEALTH_RATIO = new(
    "css_jb_rebel_t_hp_ratio", "Ratio of T : CT Health", 0.5,
    ConVarFlags.FCVAR_NONE, new RangeValidator<double>(0.00001, 10));

  public static readonly FakeConVar<int> CV_MAX_T_HEALTH =
    new("css_jb_rebel_t_max_hp", "Max HP that the rebeller can have otherwise",
      125, ConVarFlags.FCVAR_NONE, new RangeValidator<int>(1, 1000));

  public HashSet<int> PlayersLRRebelling { get; } = [];

  public void MarkLRRebelling(CCSPlayerController player) {
    MenuManager.CloseActiveMenu(player);

    int updatedHealth   = 0;
    var calculatedHealth = CalculateHealth();
    var playerPawn      = player.PlayerPawn.Value;
    if (playerPawn != null && calculatedHealth < playerPawn.Health) {
      if (playerPawn.Health > CV_MAX_T_HEALTH.Value)
        updatedHealth = CV_MAX_T_HEALTH.Value;
    } else {
      updatedHealth = calculatedHealth;
    }

    player.SetHealth(updatedHealth);
    messages.LastRequestRebel(player, updatedHealth).ToAllChat();
    AddLRRebelling(player.Slot);
    rebelService.MarkRebel(player);
    player.RemoveWeapons();
    player.GiveNamedItem(CV_REBEL_WEAPON.Value);
    player.GiveNamedItem("weapon_knife");
  }

  public bool IsInLRRebelling(int playerSlot) {
    return PlayersLRRebelling.Contains(playerSlot);
  }

  public void AddLRRebelling(int playerSlot) {
    PlayersLRRebelling.Add(playerSlot);
  }

  public void ClearLRRebelling() { PlayersLRRebelling.Clear(); }

  public int CalculateHealth() {
    var aliveCounterTerrorists = Utilities.GetPlayers()
     .Where(plr => plr is { PawnIsAlive: true, Team: CsTeam.CounterTerrorist })
     .ToList();

    return (int)Math.Floor(aliveCounterTerrorists
     .Select(player => player.PlayerPawn.Value?.Health ?? 0)
     .Select(playerHealth
        => Math.Min(playerHealth, CV_MAX_CT_HEALTH_CONTRIBUTION.Value))
     .Sum() * CV_T_HEALTH_RATIO.Value);
  }
}