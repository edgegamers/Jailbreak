using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Formatting.Views.LastRequest;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.LastRequest;

public class LastRequestCommand(ILastRequestManager lastRequestManager,
  ILastRequestRebelManager lastRequestRebelManager, ILRLocale messages,
  IGenericCmdLocale generic, ILastRequestFactory factory) : IPluginBehavior {
  private LastRequestMenuSelector? menuSelector;
  private LastRequestPlayerSelector? playerSelector;
  private BasePlugin? plugin;

  // css_lr <player> <LRType>
  public void Start(BasePlugin basePlugin) {
    plugin         = basePlugin;
    playerSelector = new LastRequestPlayerSelector(lastRequestManager, plugin);
    menuSelector   = new LastRequestMenuSelector(factory, plugin);
  }

  [ConsoleCommand("css_lr", "Start a last request as a prisoner")]
  [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
  public void Command_LastRequest(CCSPlayerController? executor,
    CommandInfo info) {
    if (executor == null || !executor.IsReal()) return;
    if (!lastRequestManager.IsLREnabled) {
      messages.LastRequestNotEnabled().ToChat(executor);
      return;
    }

    if (executor.Team != CsTeam.Terrorist) {
      messages.CannotLR("You are not a Prisoner").ToChat(executor);
      return;
    }

    if (!executor.PawnIsAlive) {
      messages.CannotLR("You are not alive").ToChat(executor);
      return;
    }


    if (!playerSelector!.WouldHavePlayers()) {
      messages.CannotLR("No players available to LR").ToChat(executor);
      return;
    }

    if (lastRequestManager.IsInLR(executor)
      || lastRequestRebelManager.IsInLRRebelling(executor.Slot)) {
      messages.CannotLR("You are already in an LR").ToChat(executor);
      return;
    }

    if (info.ArgCount == 1) {
      MenuManager.OpenCenterHtmlMenu(plugin!, executor,
        menuSelector!.GetMenu());
      return;
    }

    // Validate LR
    var type = LRTypeExtensions.FromString(info.GetArg(1));
    if (type is null) {
      messages.InvalidLastRequest(info.GetArg(1)).ToChat(executor);
      return;
    }

    if (info.ArgCount == 2) {
      MenuManager.OpenCenterHtmlMenu(plugin!, executor,
        playerSelector.CreateMenu(executor,
          str => "css_lr " + type + " #" + str));
      return;
    }

    var target = info.GetArgTargetResult(2);
    if (target.Players.Count == 0) {
      generic.PlayerNotFound(info.GetArg(2));
      return;
    }

    if (target.Players.Count > 1) {
      generic.PlayerFoundMultiple(info.GetArg(2));
      return;
    }

    var player = target.Players.First();
    if (player.Team != CsTeam.CounterTerrorist) {
      messages.CannotLR(player, "They are not a Guard").ToChat(executor);
      return;
    }

    if (!player.PawnIsAlive) {
      messages.CannotLR(player, "They are not alive").ToChat(executor);
      return;
    }

    if (lastRequestManager.IsInLR(player)) {
      messages.CannotLR(player, "They are already in an LR").ToChat(executor);
      return;
    }

    if (!lastRequestManager.InitiateLastRequest(executor, player, (LRType)type))
      info.ReplyToCommand(
        "An error occurred while initiating the last request. Please try again later.");
  }
}