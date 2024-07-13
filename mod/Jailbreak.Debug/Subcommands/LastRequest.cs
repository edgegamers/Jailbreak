using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.LastRequest;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Debug.Subcommands;

public class LastRequest : AbstractCommand {
  private readonly ILastRequestManager manager;
  private readonly LastRequestMenuSelector menuSelector;
  private readonly ILastRequestMessages messages;
  private readonly LastRequestPlayerSelector playerSelector;

  private readonly BasePlugin plugin;

  public LastRequest(IServiceProvider services, BasePlugin plugin) :
    base(services) {
    this.plugin    = plugin;
    manager        = services.GetRequiredService<ILastRequestManager>();
    playerSelector = new LastRequestPlayerSelector(manager, plugin, true);
    menuSelector = new LastRequestMenuSelector(
      services.GetRequiredService<ILastRequestFactory>(),
      type => "css_debug lastrequest " + type, plugin);
    messages = services.GetRequiredService<ILastRequestMessages>();
  }

  // (debug) lastrequest [lr] [player] <target>
  public override void OnCommand(CCSPlayerController? executor,
    WrappedInfo info) {
    if (executor == null || !executor.IsReal()) return;

    switch (info.ArgCount) {
      case 1:
        MenuManager.OpenCenterHtmlMenu(plugin, executor,
          menuSelector.GetMenu());
        return;
      case 2:
        switch (info.GetArg(1).ToLower()) {
          case "enable":
            manager.EnableLR();
            info.ReplyToCommand("Last Request enabled.");
            return;
          case "disable":
            manager.DisableLR();
            info.ReplyToCommand("Last Request disabled.");
            return;
        }

        break;
    }

    var type = LRTypeExtensions.FromString(info.GetArg(1));
    if (type is null) {
      messages.InvalidLastRequest(info.GetArg(1)).ToPlayerChat(executor);
      return;
    }

    if (info.ArgCount == 2) {
      MenuManager.OpenCenterHtmlMenu(plugin, executor,
        playerSelector.CreateMenu(executor,
          str => "css_debug lastrequest " + type + " #" + str));
      return;
    }

    var fromPlayer = GetVulnerableTarget(info, 2);
    if (fromPlayer == null) return;

    switch (info.ArgCount) {
      case 3: {
        if (executor.Team == CsTeam.Terrorist)
          manager.InitiateLastRequest(executor, fromPlayer.First(), type.Value);
        else // They aren't necessarily on different teams, but this is debug so that's OK
          manager.InitiateLastRequest(fromPlayer.First(), executor, type.Value);
        return;
      }
      case 4: {
        var targetPlayer = GetVulnerableTarget(info, 3);
        if (targetPlayer == null) return;
        manager.InitiateLastRequest(fromPlayer.First(), targetPlayer.First(),
          type.Value);
        break;
      }
    }
  }
}