using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views.Warden;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.Warden;
using Jailbreak.Public.Mod.Zones;
using Jailbreak.Public.Utils;

namespace Jailbreak.Warden.Commands;

public class WardenOpenCommandsBehavior(IWardenService warden,
  IWardenLocale msg, IWardenCmdOpenLocale wardenCmdOpenMsg,
  IZoneManager zoneManager) : IPluginBehavior, IWardenOpenCommand {
  public static readonly FakeConVar<int> CV_OPEN_COMMAND_COOLDOWN = new(
    "css_jb_warden_open_cooldown",
    "Minimum seconds warden must wait before being able to open the cells.", 20,
    customValidators: new RangeValidator<int>(0, 300));

  public bool OpenedCells { get; set; }

  [GameEventHandler]
  public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info) {
    OpenedCells = false;
    return HookResult.Continue;
  }

  [ConsoleCommand("css_open", "Opens the cell doors")]
  [ConsoleCommand("css_o", "Opens the cell doors")]
  public void Command_Open(CCSPlayerController? executor, CommandInfo info) {
    if (executor != null
      && !AdminManager.PlayerHasPermissions(executor, "@css/cheat")) {
      if (!warden.IsWarden(executor)) {
        msg.NotWarden.ToChat(executor);
        return;
      }

      if (RoundUtil.GetTimeElapsed() < CV_OPEN_COMMAND_COOLDOWN.Value) {
        wardenCmdOpenMsg.CannotOpenYet(CV_OPEN_COMMAND_COOLDOWN.Value)
         .ToChat(executor);
        return;
      }

      if (OpenedCells) {
        wardenCmdOpenMsg.AlreadyOpened.ToChat(executor);
        return;
      }
    }

    OpenedCells = true;
    var   result = MapUtil.OpenCells(zoneManager);
    IView message;
    if (result) {
      if (executor != null && !warden.IsWarden(executor))
        message = wardenCmdOpenMsg.CellsOpenedBy(executor);
      else
        message = wardenCmdOpenMsg.CellsOpenedBy(null);
    } else { message = wardenCmdOpenMsg.OpeningFailed; }

    message.ToAllChat();
  }
}