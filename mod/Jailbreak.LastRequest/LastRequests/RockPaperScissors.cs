using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Timers;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.LastRequest.LastRequests;

public class RockPaperScissors : AbstractLastRequest
{
    private ChatMenu _chatMenu;
    private int _prisonerChoice = -1, _guardChoice = -1;

    public RockPaperScissors(BasePlugin plugin, ILastRequestManager manager, CCSPlayerController prisoner,
        CCSPlayerController guard) : base(plugin, manager, prisoner, guard)
    {
        _chatMenu = new ChatMenu("Rock Paper Scissors");
        foreach (var option in new[] { "Rock", "Paper", "Scissors" })
            _chatMenu.AddMenuOption(option, OnSelect);
    }

    public override void Setup()
    {
        _chatMenu.Title = $"Rock Paper Scissors - {prisoner.PlayerName} vs {guard.PlayerName}";
        _prisonerChoice = -1;
        _guardChoice = -1;
        plugin.AddTimer(3, Execute);
    }

    private void OnSelect(CCSPlayerController player, ChatMenuOption option)
    {
        if (player.Slot != prisoner.Slot && player.Slot != guard.Slot)
            return;
        MenuManager.CloseActiveMenu(player);

        int choice = Array.IndexOf(new[] { "Rock", "Paper", "Scissors" }, option.Text);

        if (player.Slot == prisoner.Slot)
            _prisonerChoice = choice;
        else
            _guardChoice = choice;

        if (_prisonerChoice == -1 || _guardChoice == -1)
        {
            PrintToParticipants(player.PlayerName + " has made their choice...");
            return;
        }

        PrintToParticipants("Both players have made their choice!");
        if (_prisonerChoice == _guardChoice)
        {
            PrintToParticipants("It's a tie!");
            Setup();
            return;
        }

        if (state != LRState.Active)
            return;

        if (_prisonerChoice == 0 && _guardChoice == 2 || _prisonerChoice == 1 && _guardChoice == 0 ||
            _prisonerChoice == 2 && _guardChoice == 1)
            manager.EndLastRequest(this, LRResult.PrisonerWin);
        else
            manager.EndLastRequest(this, LRResult.GuardWin);
    }

    public override LRType type => LRType.RockPaperScissors;

    public override void Execute()
    {
        state = LRState.Active;
        MenuManager.OpenChatMenu(prisoner, _chatMenu);
        MenuManager.OpenChatMenu(guard, _chatMenu);

        plugin.AddTimer(20, Timeout, TimerFlags.STOP_ON_MAPCHANGE);
    }

    private void Timeout()
    {
        if (state != LRState.Active)
            return;
        if (_prisonerChoice != -1)
        {
            manager.EndLastRequest(this, LRResult.PrisonerWin);
        }
        else if (_guardChoice != -1)
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
        state = LRState.Completed;
        if (result == LRResult.GuardWin)
        {
            prisoner.Pawn.Value!.CommitSuicide(false, true);
        }
        else if (result == LRResult.PrisonerWin)
        {
            guard.Pawn.Value!.CommitSuicide(false, true);
        }

        PrintToParticipants($"Prisoner chose {GetChoice(_prisonerChoice)}, Guard chose {GetChoice(_guardChoice)}");
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