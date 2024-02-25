using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Menu;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.LastRequest;

public class LastRequestMenuSelector
{
    private readonly CenterHtmlMenu menu;
    private Func<LRType, string> command;

    public LastRequestMenuSelector() : this((lr) => "css_lr " + ((int)lr))
    {
    }

    public LastRequestMenuSelector(Func<LRType, string> command)
    {
        this.command = command;
        menu = new CenterHtmlMenu("css_lr [LR] [Player]");
        foreach (LRType lr in Enum.GetValues(typeof(LRType)))
        {
            menu.AddMenuOption(lr.ToFriendlyString(), (p, o) => OnSelectLR(p, lr));
        }
    }

    public CenterHtmlMenu GetMenu()
    {
        return menu;
    }

    private void OnSelectLR(CCSPlayerController player, LRType lr)
    {
        player.ExecuteClientCommandFromServer(this.command.Invoke(lr));
    }
}