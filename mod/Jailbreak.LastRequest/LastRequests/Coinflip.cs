using System;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views.LastRequest;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;
using Microsoft.Extensions.DependencyInjection;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.LastRequest.LastRequests;

public class Coinflip : AbstractLastRequest {
  private readonly string[] events = [
    "A glint of silver flashes through the air...", "The coin does a 180...!",
    "Gravity seems so much heavier...", "A quiet clink is heard...",
    "An arrow hits the coin!", "The coin is shot in mid-air.",
    "The answer is 42", "And yet...", "A sliver of copper falls off...",
    "Lucky number 7...", "The coin lands on its side!",
    "A bald eagle soars above",
    "There wasn't enough room for the two of ya anyways...",
    "Woosh woosh woosh", "banana rotate"
  ];

  private readonly ChatMenu menu;
  private readonly ILRCFLocale msg;
  private readonly Random rnd;
  private Timer? timeout;

  public Coinflip(BasePlugin plugin, IServiceProvider provider,
    CCSPlayerController prisoner, CCSPlayerController guard) : base(plugin,
    provider.GetRequiredService<ILastRequestManager>(), prisoner, guard) {
    rnd  = new Random();
    menu = new ChatMenu("Heads or Tails?");
    menu.AddMenuOption("Heads", (_, _) => decide(true, true));
    menu.AddMenuOption("Tails", (_, _) => decide(false, true));
    msg = provider.GetRequiredService<ILRCFLocale>();
  }

  public override LRType Type => LRType.COINFLIP;

  public override void Setup() {
    State = LRState.PENDING;
    menu.Title = "Heads or Tails? - " + Prisoner.PlayerName + " vs "
      + Guard.PlayerName;
    Execute();
  }

  public override void Execute() {
    State = LRState.ACTIVE;
    MenuManager.OpenChatMenu(Guard, menu);

    timeout = Plugin.AddTimer(10, () => {
      if (State != LRState.ACTIVE) return;
      MenuManager.CloseActiveMenu(Guard);
      var choice = rnd.Next(2) == 1;
      msg.FailedToChooseInTime(choice).ToChat(Guard);
      decide(choice, true);
    });
  }

  private void decide(bool heads, bool print) {
    timeout?.Kill();
    if (print) {
      MenuManager.CloseActiveMenu(Guard);
      msg.GuardChose(Guard, heads).ToChat(Guard, Prisoner);
      State = LRState.ACTIVE;
    }

    Plugin.AddTimer(2, () => {
      if (rnd.Next(4) == 0) {
        PrintToParticipants(events[rnd.Next(events.Length)]);
        Plugin.AddTimer(2, () => decide(heads, false));
      } else {
        var side = rnd.Next(2) == 1;
        msg.CoinLandsOn(side).ToChat(Guard, Prisoner);
        Manager.EndLastRequest(this,
          side == heads ? LRResult.GUARD_WIN : LRResult.PRISONER_WIN);
      }
    });
  }

  public override void OnEnd(LRResult result) {
    State = LRState.COMPLETED;
    if (result == LRResult.PRISONER_WIN)
      Guard.Pawn.Value?.CommitSuicide(false, true);
    else
      Prisoner.Pawn.Value?.CommitSuicide(false, true);
  }
}