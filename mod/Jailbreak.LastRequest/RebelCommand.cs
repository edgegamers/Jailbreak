using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views.LastRequest;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.Rebel;
using Jailbreak.Validator;

namespace Jailbreak.LastRequest;

public class LastRequestRebelCommand(IRebelService rebelService, ILastRequestManager manager,
  ILRLocale messages) : IPluginBehavior {
  private readonly HashSet<int> players_rebelling = new HashSet<int>();

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

  private int calculateHealth() {
    var aliveCounterTerrorists = Utilities.GetPlayers()
     .Where(plr => plr is { PawnIsAlive: true, Team: CsTeam.CounterTerrorist })
     .ToList();

    return (int)Math.Floor(aliveCounterTerrorists
     .Select(player => player.PlayerPawn.Value?.Health ?? 0)
     .Select(playerHealth
        => Math.Min(playerHealth, CV_MAX_CT_HEALTH_CONTRIBUTION.Value))
     .Sum() * CV_T_HEALTH_RATIO.Value);
  }

  [ConsoleCommand("css_rebel", "Rebel during last request as a prisoner")]
  [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
  public void Command_Rebel(CCSPlayerController? rebeller, CommandInfo info) {
    if (rebeller == null || !rebeller.IsReal()) return;
    if (!CV_REBEL_ON.Value) {
      messages.LastRequestRebelDisabled().ToChat(rebeller);
      return;
    }

    if (rebeller.Team != CsTeam.Terrorist) {
      messages.CannotLastRequestRebelCt().ToChat(rebeller);
      return;
    }

    if (!manager.IsLREnabled) {
      messages.LastRequestNotEnabled().ToChat(rebeller);
      return;
    }

    if (manager.IsInLR(rebeller)) {
      messages.CannotLastRequestRebelTActive().ToChat(rebeller);
      return;
    }

    if (players_rebelling.Contains(rebeller.Slot)) {
      messages.CannotLastRequestRebelT().ToChat(rebeller);
      return;
    }

    var calculated         = calculateHealth();
    var rebellerPlayerPawn = rebeller.PlayerPawn.Value;
    if (calculated < rebellerPlayerPawn.Health) {
      if (rebellerPlayerPawn.Health > CV_MAX_T_HEALTH.Value)
        rebellerPlayerPawn.Health = CV_MAX_T_HEALTH.Value;
    } else { rebellerPlayerPawn.Health = calculated; }
    
    MenuManager.CloseActiveMenu(rebeller);
    messages.LastRequestRebel(rebeller, rebellerPlayerPawn.Health).ToAllChat();
    players_rebelling.Add(rebeller.Slot);
    rebelService.MarkRebel(rebeller);
    rebeller.SetColor(Color.Red);
    rebeller.RemoveWeapons();
    rebeller.GiveNamedItem(CV_REBEL_WEAPON.Value);
    rebeller.GiveNamedItem("weapon_knife");
  }

  [GameEventHandler]
  public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info) {
    players_rebelling.Clear();
    return HookResult.Continue;
  }
}