using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Timers;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views.LastRequest;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;
using Jailbreak.Public.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.LastRequest.LastRequests;

public class RockPaperScissors : AbstractLastRequest {
  private readonly ChatMenu chatMenu;
  private readonly ILRRPSLocale msg;
  private int prisonerChoice = -1, guardChoice = -1;

  public RockPaperScissors(BasePlugin plugin, IServiceProvider provider,
    CCSPlayerController prisoner, CCSPlayerController guard) : base(plugin,
    provider.GetRequiredService<ILastRequestManager>(), prisoner, guard) {
    chatMenu = new ChatMenu("Rock Paper Scissors");
    msg      = provider.GetRequiredService<ILRRPSLocale>();
    foreach (var option in new[] { "Rock", "Paper", "Scissors" })
      chatMenu.AddMenuOption(option, OnSelect);
  }

  public override LRType Type => LRType.ROCK_PAPER_SCISSORS;

  public override void Setup() {
    chatMenu.Title =
      $"Rock Paper Scissors - {Prisoner.PlayerName} vs {Guard.PlayerName}";
    prisonerChoice = -1;
    guardChoice    = -1;
    Plugin.AddTimer(3, Execute);
  }

  private void OnSelect(CCSPlayerController player, ChatMenuOption option) {
    if (player.Slot != Prisoner.Slot && player.Slot != Guard.Slot) return;
    MenuManager.CloseActiveMenu(player);

    var choice = Array.IndexOf(["Rock", "Paper", "Scissors"], option.Text);

    if (player.Slot == Prisoner.Slot)
      prisonerChoice = choice;
    else
      guardChoice = choice;

    if (prisonerChoice == -1 || guardChoice == -1) {
      msg.PlayerMadeChoice(player).ToChat(Prisoner, Guard);
      return;
    }

    msg.BothPlayersMadeChoice().ToChat(Prisoner, Guard);
    if (prisonerChoice == guardChoice) {
      msg.Tie().ToAllChat();
      Setup();
      return;
    }

    if (State != LRState.ACTIVE) return;

    if (prisonerChoice == 0 && guardChoice == 2
      || prisonerChoice == 1 && guardChoice == 0
      || prisonerChoice == 2 && guardChoice == 1)
      Manager.EndLastRequest(this, LRResult.PRISONER_WIN);
    else
      Manager.EndLastRequest(this, LRResult.GUARD_WIN);
  }

  public override void Execute() {
    State = LRState.ACTIVE;
    MenuManager.OpenChatMenu(Prisoner, chatMenu);
    MenuManager.OpenChatMenu(Guard, chatMenu);

    Plugin.AddTimer(Math.Min(RoundUtil.GetTimeRemaining() - 1, 25), timeout,
      TimerFlags.STOP_ON_MAPCHANGE);
  }

  private void timeout() {
    if (State != LRState.ACTIVE) return;
    if (prisonerChoice != -1)
      Manager.EndLastRequest(this, LRResult.PRISONER_WIN);
    else if (guardChoice != -1)
      Manager.EndLastRequest(this, LRResult.GUARD_WIN);
    else
      Manager.EndLastRequest(this, LRResult.TIMED_OUT);
  }

  public override void OnEnd(LRResult result) {
    State = LRState.COMPLETED;
    switch (result) {
      case LRResult.GUARD_WIN:
        Prisoner.Pawn.Value!.CommitSuicide(false, true);
        break;
      case LRResult.PRISONER_WIN:
        Guard.Pawn.Value!.CommitSuicide(false, true);
        break;
    }

    msg.Results(Guard, Prisoner, guardChoice, prisonerChoice).ToAllChat();
  }
}