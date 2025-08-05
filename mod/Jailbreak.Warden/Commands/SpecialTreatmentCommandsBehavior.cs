using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Formatting.Views.Warden;
using Jailbreak.Public;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.Warden;
using MStatsShared;

namespace Jailbreak.Warden.Commands;

public class SpecialTreatmentCommandsBehavior(IWardenService warden,
  ISpecialTreatmentService specialTreatment, IGenericCmdLocale generic,
  IWardenLocale wardenNotifs) : IPluginBehavior {
  [ConsoleCommand("css_treat",
    "Grant or revoke special treatment from a player")]
  [ConsoleCommand("css_st", "Grant or revoke special treatment from a player")]
  [CommandHelper(0, "<target>", CommandUsage.CLIENT_ONLY)]
  public void Command_Toggle(CCSPlayerController? player, CommandInfo command) {
    if (player == null) return;

    if (!warden.IsWarden(player)) {
      wardenNotifs.NotWarden.ToChat(player).ToConsole(player);
      return;
    }

    if (command.ArgCount == 1)
      // TODO: Pop up menu of prisoners to toggle ST for
      return;

    // FIXME: Remove after @aim is fixed in CSS
    if (command.ArgByIndex(1).ToLower().Contains("@aim")) {
      command.ReplyToCommand("@aim is currently not supported due to a bug in CSS.");
      return;
    }

    var targets = command.GetArgTargetResult(1);
    var eligible = targets
     .Where(p => p is { Team: CsTeam.Terrorist, PawnIsAlive: true })
     .ToList();

    if (eligible.Count == 0) {
      generic.PlayerNotFound(command.GetArg(1))
       .ToChat(player)
       .ToConsole(player);
      return;
    }

    if (eligible.Count != 1) {
      generic.PlayerFoundMultiple(command.GetArg(1))
       .ToChat(player)
       .ToConsole(player);
      return;
    }

    //	One target, mark as ST.
    var special = eligible.First();
    API.Stats?.PushStat(new ServerStat("JB_SPECIALTREATMENT",
      special.SteamID.ToString()));

    specialTreatment.SetSpecialTreatment(special,
      !specialTreatment.IsSpecialTreatment(special));
  }
}