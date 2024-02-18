using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Commands.Targeting;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;
using Microsoft.Extensions.DependencyModel;

namespace Jailbreak.LastRequest;

public class LastRequestCommand : IPluginBehavior
{

    private ILastRequestManager _lrManager;
    private LastRequestMenuSelector menuSelector;
    private LastRequestPlayerSelector playerSelector;
    private BasePlugin plugin;

    // css_lr <player> <LRType>
    public LastRequestCommand(ILastRequestManager manager, ILastRequestFactory factory)
    {
        _lrManager = manager;
    }

    public void Start(BasePlugin plugin)
    {
        this.plugin = plugin;
        playerSelector = new LastRequestPlayerSelector(_lrManager);
        menuSelector = new LastRequestMenuSelector();
    }


    [ConsoleCommand("css_lr", "Start a last request as a prisoner")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void Command_LastRequest(CCSPlayerController? executor, CommandInfo info)
    {
        if (executor == null || !executor.IsReal())
            return;
        if (executor.Team != CsTeam.Terrorist)
        {
            info.ReplyToCommand("You must be a terrorist to LR.");
            return;
        }

        if (!executor.PawnIsAlive)
        {
            info.ReplyToCommand("You must be alive to LR.");
            return;
        }

        if (!_lrManager.IsLREnabled)
        {
            info.ReplyToCommand("LR is not yet enabled!");
            return;
        }

        if (info.ArgCount == 1)
        {
            MenuManager.OpenCenterHtmlMenu(plugin, executor, menuSelector.GetMenu());
            return;
        }

        // Validate LR
        LRType? type = LRTypeExtensions.FromString(info.GetArg(1));
        if (type == null)
        {
            info.ReplyToCommand("Invalid LR");
            return;
        }
        if (info.ArgCount == 2)
        {
            MenuManager.OpenCenterHtmlMenu(plugin, executor, playerSelector.CreateMenu(executor, (LRType)type));
            return;
        }

        var target = info.GetArgTargetResult(2);
        if (!target.Players.Any())
        {
            info.ReplyToCommand("Invalid player");
            return;
        }

        if (target.Players.Count() > 1)
        {
            info.ReplyToCommand("Too many players");
            return;
        }

        var player = target.Players.First();
        if (player.Team != CsTeam.CounterTerrorist)
        {
            info.ReplyToCommand("Invalid player");
            return;
        }

        _lrManager.InitiateLastRequest(executor, player, (LRType)type);
    }
}