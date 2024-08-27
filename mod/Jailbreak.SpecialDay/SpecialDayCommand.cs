using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Menu;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views.SpecialDay;
using Jailbreak.Formatting.Views.Warden;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;
using Jailbreak.Public.Mod.Warden;
using Jailbreak.Public.Utils;
using Jailbreak.SpecialDay.SpecialDays;

namespace Jailbreak.SpecialDay;

public class SpecialDayCommand(IWardenService warden,
  ISpecialDayFactory factory, IWardenLocale wardenMsg, ISDLocale sdMsg,
  ISpecialDayManager sd) : IPluginBehavior {
  public static FakeConVar<int> CvRoundsBetweenSD = new(
    "css_jb_sd_round_cooldown", "Rounds between special days", 4);

  public static FakeConVar<int> CvMaxElapsedTime = new(
    "css_jb_sd_max_elapsed_time",
    "Max time elapsed in a round to be able to call a special day", 30);

  private SpecialDayMenuSelector menuSelector = null!;
  private BasePlugin plugin = null!;

  // css_lr <player> <LRType>
  public void Start(BasePlugin basePlugin) {
    plugin       = basePlugin;
    menuSelector = new SpecialDayMenuSelector(factory, plugin);
  }

  [ConsoleCommand("css_sd", "Start a special day as the warden")]
  [ConsoleCommand("css_specialday", "Start a special day as the warden")]
  [ConsoleCommand("css_startday", "Start a special day as the warden")]
  public void Command_SpecialDay(CCSPlayerController? executor,
    CommandInfo info) {
    if (executor != null
      && !AdminManager.PlayerHasPermissions(executor, "@css/rcon")) {
      if (!warden.IsWarden(executor) || RoundUtil.IsWarmup()) {
        wardenMsg.NotWarden.ToChat(executor);
        return;
      }

      if (sd.IsSDRunning) {
        // SD is already running
        if (sd.CurrentSD is ISpecialDayMessageProvider messaged)
          sdMsg.SpecialDayRunning(messaged.Locale.Name).ToChat(executor);
        else
          sdMsg.SpecialDayRunning(sd.CurrentSD?.Type.ToString() ?? "Unknown")
           .ToChat(executor);

        return;
      }

      var roundsToNext = sd.RoundsSinceLastSD - CvRoundsBetweenSD.Value;
      if (roundsToNext < 0) {
        sdMsg.SpecialDayCooldown(Math.Abs(roundsToNext)).ToChat(executor);
        return;
      }

      if (RoundUtil.GetTimeElapsed() > CvMaxElapsedTime.Value) {
        sdMsg.TooLateForSpecialDay(CvMaxElapsedTime.Value);
        return;
      }
    }

    if (info.ArgCount == 1) {
      if (executor == null) {
        Server.PrintToConsole("css_sd [SD]");
        return;
      }

      MenuManager.OpenCenterHtmlMenu(plugin, executor, menuSelector.GetMenu());
      return;
    }

    // Validate LR
    var type = SDTypeExtensions.FromString(info.GetArg(1));
    if (type == null) {
      if (executor != null)
        sdMsg.InvalidSpecialDay(info.GetArg(1)).ToChat(executor);
      return;
    }

    sd.InitiateSpecialDay(type.Value);
  }
}