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
    
    public Coinflip(BasePlugin plugin, ILastRequestManager manager, CCSPlayerController prisoner, CCSPlayerController guard) : base(plugin, manager, prisoner, guard)
    {
        menu = new ChatMenu("Heads or Tails?");
        menu.AddMenuOption("Heads", (player, option) => Decide(option.Text.Equals("Heads")));
        rnd = new Random();
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
            if(state != LRState.Active)
                return;
            MenuManager.CloseActiveMenu(guard);
            bool choice = rnd.Next(2) == 1;
            guard.PrintToChat($"You failed to choose in time, defaulting to {(choice ? "Heads" : "Tails")}");
            Decide(choice);
        });
    }

    private void Decide(bool heads)
    {
        
    }

    public override void OnEnd(LRResult result)
    {
    }
}