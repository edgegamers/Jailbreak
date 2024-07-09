using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Timers;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.LastRequest.LastRequests;

public class RockPaperScissors : AbstractLastRequest {
  private readonly ChatMenu chatMenu;
  private int prisonerChoice = -1, guardChoice = -1;

  public RockPaperScissors(BasePlugin plugin, ILastRequestManager manager,
    CCSPlayerController prisoner, CCSPlayerController guard) : base(plugin,
    manager, prisoner, guard) {
    chatMenu = new ChatMenu("Rock Paper Scissors");
    foreach (var option in new[] { "Rock", "Paper", "Scissors" })
      chatMenu.AddMenuOption(option, OnSelect);
  }

  public override LRType type => LRType.ROCK_PAPER_SCISSORS;

  public override void Setup() {
    chatMenu.Title =
      $"Rock Paper Scissors - {prisoner.PlayerName} vs {guard.PlayerName}";
    prisonerChoice = -1;
    guardChoice    = -1;
    plugin.AddTimer(3, Execute);
  }

  private void OnSelect(CCSPlayerController player, ChatMenuOption option) {
    if (player.Slot != prisoner.Slot && player.Slot != guard.Slot) return;
    MenuManager.CloseActiveMenu(player);

    var choice = Array.IndexOf(new[] { "Rock", "Paper", "Scissors" },
      option.Text);

    if (player.Slot == prisoner.Slot)
      prisonerChoice = choice;
    else
      guardChoice = choice;

    if (prisonerChoice == -1 || guardChoice == -1) {
      PrintToParticipants(player.PlayerName + " has made their choice...");
      return;
    }

    PrintToParticipants("Both players have made their choice!");
    if (prisonerChoice == guardChoice) {
      PrintToParticipants("It's a tie!");
      Setup();
      return;
    }

    if (state != LRState.Active) return;

    if (prisonerChoice == 0 && guardChoice == 2
      || prisonerChoice == 1 && guardChoice == 0
      || prisonerChoice == 2 && guardChoice == 1)
      manager.EndLastRequest(this, LRResult.PrisonerWin);
    else
      manager.EndLastRequest(this, LRResult.GuardWin);
  }

  public override void Execute() {
    state = LRState.Active;
    MenuManager.OpenChatMenu(prisoner, chatMenu);
    MenuManager.OpenChatMenu(guard, chatMenu);

    plugin.AddTimer(20, timeout, TimerFlags.STOP_ON_MAPCHANGE);
  }

  private void timeout() {
    if (state != LRState.Active) return;
    if (prisonerChoice != -1)
      manager.EndLastRequest(this, LRResult.PrisonerWin);
    else if (guardChoice != -1)
      manager.EndLastRequest(this, LRResult.GuardWin);
    else
      manager.EndLastRequest(this, LRResult.TimedOut);
  }

  public override void OnEnd(LRResult result) {
    state = LRState.Completed;
    if (result == LRResult.GuardWin)
      prisoner.Pawn.Value!.CommitSuicide(false, true);
    else if (result == LRResult.PrisonerWin)
      guard.Pawn.Value!.CommitSuicide(false, true);

    PrintToParticipants(
      $"Prisoner chose {getChoice(prisonerChoice)}, Guard chose {getChoice(guardChoice)}");
  }

  private string getChoice(int choice) {
    return choice switch {
      0 => "Rock",
      1 => "Paper",
      2 => "Scissors",
      _ => "Unknown"
    };
  }
}