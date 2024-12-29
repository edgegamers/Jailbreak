using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Menu;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views.SpecialDay;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;
using Jailbreak.SpecialDay.SpecialDays;

namespace Jailbreak.SpecialDay;

public class SpecialDayCommand(ISpecialDayFactory factory, ISDLocale sdMsg,
  ISpecialDayManager sd) : IPluginBehavior {
  public static readonly FakeConVar<int> CV_ROUNDS_BETWEEN_SD = new(
    "css_jb_sd_round_cooldown", "Rounds between special days", 5);

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
    if (executor != null && sd.IsSDRunning && info.ArgCount == 1) {
      // SD is already running
      if (sd.CurrentSD is ISpecialDayMessageProvider messaged)
        sdMsg.SpecialDayRunning(messaged.Locale.Name).ToChat(executor);
      else
        sdMsg.SpecialDayRunning(sd.CurrentSD?.Type.ToString() ?? "Unknown")
         .ToChat(executor);

      return;
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

    var canStart = sd.CanStartSpecialDay(type.Value, executor);
    if (canStart != null) {
      if (executor != null) executor.PrintToChat(canStart);
      return;
    }

    sd.InitiateSpecialDay(type.Value);
  }
}