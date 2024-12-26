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

  public void StartLRRebelling(CCSPlayerController player) {
    MenuManager.CloseActiveMenu(player);
    var finalRebelHealth = DetermineFinalRebelHealth(player);
    messages.LastRequestRebel(player, finalRebelHealth).ToAllChat();
    rebelService.MarkRebel(player);
    AddLRRebelling(player.Slot);
    player.SetHealth(finalRebelHealth);
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

  public void ClearLRRebelling() {
    PlayersLRRebelling.Clear();
  }

  public int DetermineFinalRebelHealth(CCSPlayerController player) {
    var calculatedRebelHealthRatio = CalculateRebelHealthRatio();
    var playerPawn = player.PlayerPawn.Value;
    if (playerPawn != null) {
      if (calculatedRebelHealthRatio <= playerPawn.Health && playerPawn.Health >= CV_MAX_T_HEALTH.Value) {
        return CV_MAX_T_HEALTH.Value;
      }
      if (calculatedRebelHealthRatio <= playerPawn.Health) {
        return playerPawn.Health;
      }
      if (calculatedRebelHealthRatio >= playerPawn.Health) {
        return calculatedRebelHealthRatio;
      }
    }
    return 101;
  }

  public int CalculateRebelHealthRatio() {
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