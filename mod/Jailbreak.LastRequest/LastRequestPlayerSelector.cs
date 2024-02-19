using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.LastRequest;

public class LastRequestPlayerSelector
{
    private ILastRequestManager _lrManager;

    public LastRequestPlayerSelector(ILastRequestManager manager)
    {
        _lrManager = manager;
    }

    public CenterHtmlMenu CreateMenu(CCSPlayerController player, LRType lrType)
    {
        CenterHtmlMenu menu = new CenterHtmlMenu("Player?");

        foreach (var target in Utilities.GetPlayers())
        {
            if (!target.IsReal())
                continue;
            if (!target.PawnIsAlive || target.Team != CsTeam.CounterTerrorist)
                continue;
            menu.AddMenuOption(target.PlayerName,
                (player, option) => OnSelect(player, option, lrType, target),
                _lrManager.IsInLR(target)
            );
        }

        return menu;
    }

    private void OnSelect(CCSPlayerController player, ChatMenuOption option, LRType lr, CCSPlayerController target)
    {
        player.ExecuteClientCommandFromServer("css_lr " + ((int) lr) + " #" + target.UserId);
    }
}