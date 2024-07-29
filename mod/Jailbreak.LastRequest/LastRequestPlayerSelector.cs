using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Mod.LastRequest;

namespace Jailbreak.LastRequest;

public class LastRequestPlayerSelector(ILastRequestManager manager,
  BasePlugin plugin, bool debug = false) {
  public CenterHtmlMenu CreateMenu(CCSPlayerController player,
    Func<string?, string> command) {
    var menu = new CenterHtmlMenu(command.Invoke("[Player]"), plugin);

    foreach (var target in Utilities.GetPlayers()
     .Where(target => target.PawnIsAlive
        && (target.Team == CsTeam.CounterTerrorist || debug)))
      menu.AddMenuOption(target.PlayerName,
        (_, _) => onSelect(player, command, target.UserId.ToString()),
        !debug && manager.IsInLR(target));

    return menu;
  }

  public bool WouldHavePlayers() {
    return Utilities.GetPlayers()
     .Any(p => p is { PawnIsAlive: true, Team: CsTeam.CounterTerrorist });
  }

  private void onSelect(CCSPlayerController player,
    Func<string?, string> command, string? value) {
    MenuManager.CloseActiveMenu(player);
    player.ExecuteClientCommandFromServer(command.Invoke(value));
  }
}