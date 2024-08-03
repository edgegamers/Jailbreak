using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.Warden;
using Jailbreak.Public.Mod.Zones;
using Jailbreak.Public.Utils;

namespace Jailbreak.Warden.Commands;

public class OpenCommandsBehavior(IWardenService warden, IWardenLocale msg,
  IWardenCmdOpenLocale wardenCmdOpenMsg, IZoneManager zoneManager)
  : IPluginBehavior {
  public static readonly FakeConVar<int> CvOpenCommandCooldown = new(
    "css_jb_warden_open_cooldown",
    "Minimum seconds warden must wait before being able to open the cells.", 30,
    customValidators: new RangeValidator<int>(0, 300));

  [ConsoleCommand("css_open", "Opens the cell doors")]
  [ConsoleCommand("css_o", "Opens the cell doors")]
  public void Command_Open(CCSPlayerController? executor, CommandInfo info) {
    if (executor != null
      && !AdminManager.PlayerHasPermissions(executor, "@css/cheat"))
      if (!warden.IsWarden(executor)) {
        msg.NotWarden.ToChat(executor);
        return;
      }

    if (RoundUtil.GetTimeElapsed() < CvOpenCommandCooldown.Value) {
      wardenCmdOpenMsg.CannotOpenYet(CvOpenCommandCooldown.Value);
      return;
    }

    var result = MapUtil.OpenCells(zoneManager);
    (result ? wardenCmdOpenMsg.CellsOpened : wardenCmdOpenMsg.OpeningFailed)
     .ToAllChat();
  }
}