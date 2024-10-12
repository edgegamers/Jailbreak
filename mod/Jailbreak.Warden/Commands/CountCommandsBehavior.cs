using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views.Warden;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.Warden;
using Jailbreak.Public.Utils;
using Microsoft.VisualBasic;

namespace Jailbreak.Warden.Commands;

public class CountCommandsBehavior(IWardenService warden, IWardenLocale msg,
  IWardenCmdCountLocale locale, IMarkerService markers) : IPluginBehavior {
  public static readonly FakeConVar<int> CV_COUNT_COMMAND_COOLDOWN = new(
    "css_jb_warden_count_cooldown",
    "Minimum seconds warden must wait before being able to count perisoners in marker.",
    30, customValidators: new RangeValidator<int>(0, 300));

  [ConsoleCommand("css_count", "Counts the prisoners in marker")]
  public void Command_Count(CCSPlayerController? executor, CommandInfo info) {
    if (executor == null) return;
    if (!warden.IsWarden(executor)) {
      msg.NotWarden.ToChat(executor);
      return;
    }

    if (RoundUtil.GetTimeElapsed() < CV_COUNT_COMMAND_COOLDOWN.Value) {
      locale.CannotCountYet(CV_COUNT_COMMAND_COOLDOWN.Value).ToChat(executor);
      return;
    }

    if (markers.MarkerPosition == null) {
      locale.NoMarkerSet.ToChat(executor);
      return;
    }

    var prisoners = PlayerUtil.FromTeam(CsTeam.Terrorist)
     .Count(markers.InMarker);

    locale.PrisonersInMarker(prisoners);
  }
}