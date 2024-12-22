using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views.LastRequest;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastRequest;

namespace Jailbreak.LastRequest;

public class LastRequestRebelCommand(ILastRequestManager lastRequestManager,
  ILastRequestRebelManager lastRequestRebelManager, ILRLocale messages)
  : IPluginBehavior {
  [ConsoleCommand("css_rebel", "Rebel during last request as a prisoner")]
  [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
  public void Command_Rebel(CCSPlayerController? rebeller, CommandInfo info) {
    if (rebeller == null || !rebeller.IsReal()) return;
    if (!LastRequestRebelManager.CV_REBEL_ON.Value) {
      messages.LastRequestRebelDisabled().ToChat(rebeller);
      return;
    }

    if (rebeller.Team != CsTeam.Terrorist) {
      messages.CannotLastRequestRebelCt().ToChat(rebeller);
      return;
    }

    if (!lastRequestManager.IsLREnabled) {
      messages.LastRequestNotEnabled().ToChat(rebeller);
      return;
    }

    if (lastRequestManager.IsInLR(rebeller) || lastRequestRebelManager.IsInLRRebelling(rebeller.Slot)) {
      messages.CannotLR("You are already in an LR").ToChat(rebeller);
      return;
    }

    lastRequestRebelManager.MarkLRRebelling(rebeller);
  }

  [GameEventHandler]
  public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info) {
    lastRequestRebelManager.ClearLRRebelling();
    return HookResult.Continue;
  }
}