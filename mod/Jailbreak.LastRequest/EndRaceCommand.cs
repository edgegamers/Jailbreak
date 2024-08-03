using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views.LastRequest;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.LastRequest;

public class EndRaceCommand(ILastRequestManager lrManager,
  ILRRaceLocale messages) : IPluginBehavior {
  [ConsoleCommand("css_endrace", "Used to set the end point of a race LR")]
  [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
  public void Command_EndRace(CCSPlayerController? executor, CommandInfo info) {
    if (executor == null) return;
    var lr = lrManager.GetActiveLR(executor);

    if (lr is not { Type: LRType.RACE }) {
      messages.NotInRaceLR().ToChat(executor);
      return;
    }

    if (lr.State != LRState.PENDING) {
      messages.NotInPendingState().ToChat(executor);
      return;
    }

    lr.Execute();
  }
}