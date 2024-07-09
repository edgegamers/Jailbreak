using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.LastRequest;

public class LastRequestMenuSelector
{
    private readonly Func<LRType, string> _command;
    private readonly CenterHtmlMenu _menu;

    public LastRequestMenuSelector(ILastRequestFactory factory) : this(factory, lr => "css_lr " + (int)lr)
    {
    }

    public LastRequestMenuSelector(ILastRequestFactory factory, Func<LRType, string> command)
    {
        _command = command;
        _menu = new CenterHtmlMenu("css_lr [LR] [Player]");
        foreach (LRType lr in Enum.GetValues(typeof(LRType)))
        {
            if (!factory.IsValidType(lr))
                continue;
            _menu.AddMenuOption(lr.ToFriendlyString(), (p, o) => OnSelectLR(p, lr));
        }
    }

    public CenterHtmlMenu GetMenu()
    {
        return _menu;
    }

    private void OnSelectLR(CCSPlayerController player, LRType lr)
    {
        MenuManager.CloseActiveMenu(player);
        player.ExecuteClientCommandFromServer(_command.Invoke(lr));
    }
}