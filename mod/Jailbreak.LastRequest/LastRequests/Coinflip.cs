using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.LastRequest.LastRequests;

public class Coinflip : AbstractLastRequest
{
    private ChatMenu menu;
    private Random rnd;

    public Coinflip(BasePlugin plugin, ILastRequestManager manager, CCSPlayerController prisoner,
        CCSPlayerController guard) : base(plugin, manager, prisoner, guard)
    {
        rnd = new Random();
        menu = new ChatMenu("Heads or Tails?");
        menu.AddMenuOption("Heads", (_, option) => Decide(option.Text.Equals("Heads"), true));
    }

    public override LRType type => LRType.Coinflip;

    public override void Setup()
    {
        state = LRState.Pending;
        menu.Title = "Heads or Tails? - " + prisoner.PlayerName + " vs " + guard.PlayerName;
        Execute();
    }

    public override void Execute()
    {
        state = LRState.Active;
        MenuManager.OpenChatMenu(guard, menu);

        plugin.AddTimer(10, () =>
        {
            if (state != LRState.Active)
                return;
            MenuManager.CloseActiveMenu(guard);
            bool choice = rnd.Next(2) == 1;
            guard.PrintToChat($"You failed to choose in time, defaulting to {(choice ? "Heads" : "Tails")}");
            Decide(choice, true);
        });
    }

    private void Decide(bool heads, bool print)
    {
        if(print)
            PrintToParticipants($"{guard.PlayerName} chose {(heads ? "Heads" : "Tails")}... flipping...");
        plugin.AddTimer(2, () =>
        {
            if (rnd.Next(4) == 0)
            {
                PrintToParticipants(events[rnd.Next(events.Length)]);
                plugin.AddTimer(2, () => Decide(heads, false));
            }
            else
            {
                var side = rnd.Next(2) == 1;
                PrintToParticipants($"The coin lands on {(side ? "Heads" : "Tails")}!");
                manager.EndLastRequest(this, side == heads ? LRResult.PrisonerWin : LRResult.GuardWin);
            }
        });
    }

    private readonly string[] events =
    {
        "A glint of silver flashes through the air...",
        "The coin does a 180...!",
        "Gravity seems so much heavier...",
        "A quiet clink is heard...",
        "An arrow hits the coin!",
        "The coin is shot in mid-air.",
        "The answer is 42",
        "And yet...",
        "A sliver of copper falls off...",
        "Lucky number 7...",
        "The coin lands on its side!",
        "A bald eagle soars above",
        "There wasn't enough room for the two of ya anyways...",
        "Woosh woosh woosh",
        "banana rotate"
    };

    public override void OnEnd(LRResult result)
    {
        state = LRState.Completed;
        if (result == LRResult.PrisonerWin)
            guard.Pawn.Value?.CommitSuicide(false, true);
        else
            prisoner.Pawn.Value?.CommitSuicide(false, true);
    }
}