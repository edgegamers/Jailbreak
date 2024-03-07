using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Timers;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.LastRequest.LastRequests;

public class RockPaperScissors : AbstractLastRequest
{
    private ChatMenu chatMenu;
    private int prisonerChoice = -1, guardChoice = -1;

    public RockPaperScissors(BasePlugin plugin, ILastRequestManager manager, CCSPlayerController prisoner,
        CCSPlayerController guard) : base(plugin, manager, prisoner, guard)
    {
        chatMenu = new ChatMenu("Rock Paper Scissors");
        foreach (string option in new[] { "Rock", "Paper", "Scissors" })
            chatMenu.AddMenuOption(option, OnSelect);
    }

    public override void Setup()
    {
        chatMenu.Title = $"Rock Paper Scissors - {prisoner.PlayerName} vs {guard.PlayerName}";
        prisonerChoice = -1;
        guardChoice = -1;
        plugin.AddTimer(3, Execute);
    }

    private void OnSelect(CCSPlayerController player, ChatMenuOption option)
    {
        if (player.Slot != prisoner.Slot && player.Slot != guard.Slot)
            return;

        int choice = Array.IndexOf(new[] { "Rock", "Paper", "Scissors" }, option.Text);

        if (player.Slot == prisoner.Slot)
            prisonerChoice = choice;
        else
            guardChoice = choice;

        if (prisonerChoice == -1 || guardChoice == -1)
        {
            PrintToParticipants(player.PlayerName + " has made their choice...");
            return;
        }

        PrintToParticipants("Both players have made their choice!");
        if (prisonerChoice == guardChoice)
        {
            PrintToParticipants("It's a tie!");
            Setup();
            return;
        }

        if (prisonerChoice == 0 && guardChoice == 2 || prisonerChoice == 1 && guardChoice == 0 ||
            prisonerChoice == 2 && guardChoice == 1)
            manager.EndLastRequest(this, LRResult.PrisonerWin);
        else
            manager.EndLastRequest(this, LRResult.GuardWin);
    }

    public override LRType type => LRType.RockPaperScissors;

    public override void Execute()
    {
        state = LRState.Active;
        MenuManager.OpenChatMenu(prisoner, chatMenu);
        MenuManager.OpenChatMenu(guard, chatMenu);

        plugin.AddTimer(20, Timeout, TimerFlags.STOP_ON_MAPCHANGE);
    }

    private void Timeout()
    {
        if (state != LRState.Active)
            return;
        if (prisonerChoice != -1)
        {
            manager.EndLastRequest(this, LRResult.PrisonerWin);
        }
        else if (guardChoice != -1)
        {
            manager.EndLastRequest(this, LRResult.GuardWin);
        }
        else
        {
            manager.EndLastRequest(this, LRResult.TimedOut);
        }
    }

    public override void OnEnd(LRResult result)
    {
        if (result == LRResult.GuardWin)
        {
            prisoner.Pawn.Value!.CommitSuicide(false, true);
        }
        else if (result == LRResult.PrisonerWin)
        {
            guard.Pawn.Value!.CommitSuicide(false, true);
        }

        PrintToParticipants($"Prisoner chose {GetChoice(prisonerChoice)}, Guard chose {GetChoice(guardChoice)}");
        state = LRState.Completed;
    }
    
    private string GetChoice(int choice)
    {
        return choice switch
        {
            0 => "Rock",
            1 => "Paper",
            2 => "Scissors",
            _ => "Unknown"
        };
    }
}