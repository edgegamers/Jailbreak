using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.Warden;
using Jailbreak.Public.Utils;

namespace Jailbreak.Warden.Commands;

public class OpenCommandsBehavior(IWardenService warden,
  IWardenNotifications msg, IOpenCommandMessages openMsg) : IPluginBehavior {
  [ConsoleCommand("css_open", "Opens the cell doors")]
  [ConsoleCommand("css_o", "Opens the cell doors")]
  public void Command_Open(CCSPlayerController? executor, CommandInfo info) {
    if (executor != null
      && !AdminManager.PlayerHasPermissions(executor, "@css/cheat"))
      if (!warden.IsWarden(executor)) {
        msg.NotWarden.ToPlayerChat(executor);
        return;
      }

    var result = MapUtil.OpenCells();
    // openMsg.OpenedCells()
  }
}