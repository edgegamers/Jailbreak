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
    private readonly ILastRequestFactory factory;

    public LastRequestMenuSelector(ILastRequestFactory factory) : this(factory, (lr) => "css_lr " + ((int)lr))
    {
        this.factory = factory;
    }

    public LastRequestMenuSelector(ILastRequestFactory factory, Func<LRType, string> command)
    {
        this.factory = factory;
        this.command = command;
        menu = new CenterHtmlMenu("css_lr [LR] [Player]");
        foreach (LRType lr in Enum.GetValues(typeof(LRType)))
        {
            if (!factory.IsValidType(lr))
                continue;
            menu.AddMenuOption(lr.ToFriendlyString(), (p, o) => OnSelectLR(p, lr));
        }
    }

    public CenterHtmlMenu GetMenu()
    {
        return menu;
    }

    private void OnSelectLR(CCSPlayerController player, LRType lr)
    {
        MenuManager.CloseActiveMenu(player);
        player.ExecuteClientCommandFromServer(this.command.Invoke(lr));
    }
}