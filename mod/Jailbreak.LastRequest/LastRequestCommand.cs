using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Commands.Targeting;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
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
    private ILastRequestMessages _messages;
    private IGenericCommandNotifications _generic;

    // css_lr <player> <LRType>
    public LastRequestCommand(ILastRequestManager manager, ILastRequestMessages messages,
        IGenericCommandNotifications generic)
    {
        _lrManager = manager;
        _messages = messages;
        _generic = generic;
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
            _messages.LastRequestNotEnabled().ToPlayerChat(executor);
            return;
        }

        if (!playerSelector.WouldHavePlayers())
        {
            info.ReplyToCommand("There are no players available to LR.");
            return;
        }

        if (_lrManager.IsInLR(executor))
        {
            info.ReplyToCommand("You are already in an LR!");
            return;
        }

        if (info.ArgCount == 1)
        {
            MenuManager.OpenCenterHtmlMenu(plugin, executor, menuSelector.GetMenu());
            return;
        }

        // Validate LR
        var type = LRTypeExtensions.FromString(info.GetArg(1));
        if (type is null) 
        {
            _messages.InvalidLastRequest(info.GetArg(1)).ToPlayerChat(executor);
            return;
        }

        if (info.ArgCount == 2)
        {
            MenuManager.OpenCenterHtmlMenu(plugin, executor, playerSelector.CreateMenu(executor, type.Value));
            return;
        }

        var target = info.GetArgTargetResult(2);
        if (!target.Players.Any())
        {
            _generic.PlayerNotFound(info.GetArg(2));
            return;
        }

        if (target.Players.Count > 1)
        {
            _generic.PlayerFoundMultiple(info.GetArg(2));
            return;
        }

        var player = target.Players.First();
        if (player.Team != CsTeam.CounterTerrorist)
        {
            _messages.InvalidPlayerChoice(player, "They're not on CT!");
            return;
        }

        if (!player.PawnIsAlive)
        {
            _messages.InvalidPlayerChoice(player, "They're not alive!");
            return;
        }

        if (_lrManager.IsInLR(player))
        {
            _messages.InvalidPlayerChoice(player, "They're already in an LR!");
            return;
        }

        if (!_lrManager.InitiateLastRequest(executor, player, (LRType)type))
        {
            info.ReplyToCommand("An error occurred while initiating the last request. Please try again later.");
        }
    }
}