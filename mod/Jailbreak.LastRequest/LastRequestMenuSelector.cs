using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Menu;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.LastRequest;

public class LastRequestMenuSelector
{
    private readonly CenterHtmlMenu menu;

    public LastRequestMenuSelector()
    {
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
        player.ExecuteClientCommandFromServer("css_lr " + (int)lr);
    }
}