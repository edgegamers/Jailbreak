using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.LastRequest;

public class LastRequestCommand(ILastRequestManager manager,
  ILastRequestMessages messages, IGenericCommandNotifications generic,
  ILastRequestFactory factory) : IPluginBehavior {
  private LastRequestMenuSelector? menuSelector;
  private LastRequestPlayerSelector? playerSelector;
  private BasePlugin? plugin;

  // css_lr <player> <LRType>
  public void Start(BasePlugin basePlugin) {
    plugin         = basePlugin;
    playerSelector = new LastRequestPlayerSelector(manager, plugin);
    menuSelector   = new LastRequestMenuSelector(factory, plugin);
  }

  [ConsoleCommand("css_lr", "Start a last request as a prisoner")]
  [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
  public void Command_LastRequest(CCSPlayerController? executor,
    CommandInfo info) {
    if (executor == null || !executor.IsReal()) return;
    if (executor.Team != CsTeam.Terrorist) {
      info.ReplyToCommand("You must be a terrorist to LR.");
      return;
    }

    if (!executor.PawnIsAlive) {
      info.ReplyToCommand("You must be alive to LR.");
      return;
    }

    if (!manager.IsLREnabled) {
      messages.LastRequestNotEnabled().ToPlayerChat(executor);
      return;
    }

    if (!playerSelector!.WouldHavePlayers()) {
      info.ReplyToCommand("There are no players available to LR.");
      return;
    }

    if (manager.IsInLR(executor)) {
      info.ReplyToCommand("You are already in an LR!");
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
      messages.InvalidLastRequest(info.GetArg(1)).ToPlayerChat(executor);
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
      messages.InvalidPlayerChoice(player, "They're not on CT!");
      return;
    }

    if (!player.PawnIsAlive) {
      messages.InvalidPlayerChoice(player, "They're not alive!");
      return;
    }

    if (manager.IsInLR(player)) {
      messages.InvalidPlayerChoice(player, "They're already in an LR!");
      return;
    }

    //One final check in case they got the menu open in the last round
    if (!manager.IsLREnabled) {
      messages.LastRequestNotEnabled().ToPlayerChat(executor);
      return;
    }

    if (!manager.InitiateLastRequest(executor, player, (LRType)type))
      info.ReplyToCommand(
        "An error occurred while initiating the last request. Please try again later.");
  }
}