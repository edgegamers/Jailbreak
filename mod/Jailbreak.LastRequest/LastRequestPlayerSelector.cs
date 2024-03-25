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
    private bool debug;

    public LastRequestPlayerSelector(ILastRequestManager manager, bool debug = false)
    {
        _lrManager = manager;
        this.debug = debug;
    }

    public CenterHtmlMenu CreateMenu(CCSPlayerController player, Func<string?, string> command)
    {
        CenterHtmlMenu menu = new CenterHtmlMenu(command.Invoke("[Player]"));

        foreach (var target in Utilities.GetPlayers())
        {
            if (!target.IsReal())
                continue;
            if (!target.PawnIsAlive || target.Team != CsTeam.CounterTerrorist && !debug)
                continue;
            menu.AddMenuOption(target.PlayerName,
                (selector, _) =>
                    OnSelect(player, command, target.UserId.ToString()),
                !debug && _lrManager.IsInLR(target)
            );
        }

        return menu;
    }

    public bool WouldHavePlayers() => Utilities.GetPlayers()
        .Any(p => p.IsReal() && p is { PawnIsAlive: true, Team: CsTeam.CounterTerrorist });

    private void OnSelect(CCSPlayerController player, Func<string?, string> command, string? value)
    {
        MenuManager.CloseActiveMenu(player);
        player.ExecuteClientCommandFromServer(command.Invoke(value));
    }
}