using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Menu;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;
using Jailbreak.Public.Mod.Warden;
using Jailbreak.SpecialDay.SpecialDays;

namespace Jailbreak.SpecialDay;

public class SpecialDayCommand(IWardenService warden,
  ISpecialDayFactory factory, IWardenNotifications wardenMsg,
  ISpecialDayMessages sdMsg, ISpecialDayManager sd) : IPluginBehavior {
  private SpecialDayMenuSelector? menuSelector;
  private BasePlugin? plugin;

  public static FakeConVar<int> CvRoundsBetweenSD = new(
    "css_jb_sd_round_cooldown", "Rounds between special days", 5);

  // css_lr <player> <LRType>
  public void Start(BasePlugin basePlugin) {
    plugin       = basePlugin;
    menuSelector = new SpecialDayMenuSelector(factory, plugin);
  }

  [ConsoleCommand("css_sd", "Start a special day as the warden")]
  [ConsoleCommand("css_specialday", "Start a special day as the warden")]
  [ConsoleCommand("css_startday", "Start a special day as the warden")]
  [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
  public void Command_SpecialDay(CCSPlayerController? executor,
    CommandInfo info) {
    if (executor == null || !executor.IsReal()) return;

    if (!AdminManager.PlayerHasPermissions(executor, "@css/rcon")) {
      if (!warden.IsWarden(executor)) {
        wardenMsg.NotWarden.ToPlayerChat(executor);
        return;
      }

      if (sd.IsSDRunning) {
        // SD is already running
        if (sd.CurrentSD is MessagedSpecialDay messaged) {
          sdMsg.SpecialDayRunning(messaged.Messages.Name)
           .ToPlayerChat(executor);
        } else {
          sdMsg.SpecialDayRunning(sd.CurrentSD?.Type.ToString() ?? "Unknown")
           .ToPlayerChat(executor);
        }

        return;
      }

      int roundsToNext = sd.RoundsSinceLastSD - CvRoundsBetweenSD.Value;
      if (roundsToNext < 0
        && !AdminManager.PlayerHasPermissions(executor, "@css/rcon")) {
        sdMsg.SpecialDayCooldown(Math.Abs(roundsToNext)).ToPlayerChat(executor);
        return;
      }
    }

    if (info.ArgCount == 1) {
      MenuManager.OpenCenterHtmlMenu(plugin!, executor,
        menuSelector!.GetMenu());
      return;
    }

    // Validate LR
    var type = SDTypeExtensions.FromString(info.GetArg(1));
    if (type == null) {
      sdMsg.InvalidSpecialDay(info.GetArg(1)).ToPlayerChat(executor);
      return;
    }

    sd.InitiateSpecialDay(type.Value);
  }
}