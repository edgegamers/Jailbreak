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

public class LastRequest : AbstractCommand
{
    private readonly ILastRequestManager _manager;
    private readonly LastRequestMenuSelector _menuSelector;
    private readonly ILastRequestMessages _messages;
    private readonly LastRequestPlayerSelector _playerSelector;

    private readonly BasePlugin _plugin;

    public LastRequest(IServiceProvider services, BasePlugin plugin) : base(services)
    {
        _plugin = plugin;
        _manager = services.GetRequiredService<ILastRequestManager>();
        _playerSelector = new LastRequestPlayerSelector(_manager, true);
        _menuSelector = new LastRequestMenuSelector(services.GetRequiredService<ILastRequestFactory>(),
            type => "css_debug lastrequest " + type);
        _messages = services.GetRequiredService<ILastRequestMessages>();
    }

    // (debug) lastrequest [lr] [player] <target>
    public override void OnCommand(CCSPlayerController? executor, WrappedInfo info)
    {
        if (executor != null && !executor.IsReal())
            return;

        if (info.ArgCount == 1 && executor != null)
        {
            MenuManager.OpenCenterHtmlMenu(_plugin, executor, _menuSelector.GetMenu());
            return;
        }

        if (info.ArgCount == 2)
            switch (info.GetArg(1).ToLower())
            {
                case "enable":
                    _manager.EnableLR();
                    info.ReplyToCommand("Last Request enabled.");
                    return;
                case "disable":
                    _manager.DisableLR();
                    info.ReplyToCommand("Last Request disabled.");
                    return;
            }

        var type = LRTypeExtensions.FromString(info.GetArg(1));
        if (type is null)
        {
            _messages.InvalidLastRequest(info.GetArg(1)).ToPlayerChat(executor);
            return;
        }

        if (info.ArgCount == 2)
        {
            MenuManager.OpenCenterHtmlMenu(_plugin, executor,
                _playerSelector.CreateMenu(executor, str => "css_debug lastrequest " + type + " #" + str));
            return;
        }

        var fromPlayer = GetVulnerableTarget(info, 2);
        if (fromPlayer == null)
            return;

        switch (info.ArgCount)
        {
            case 3 when executor != null:
            {
                if (executor.Team == CsTeam.Terrorist)
                    _manager.InitiateLastRequest(executor, fromPlayer.First(), type.Value);
                else // They aren't necessarily on different teams, but this is debug so that's OK
                    _manager.InitiateLastRequest(fromPlayer.First(), executor, type.Value);
                return;
            }
            case 4:
            {
                var targetPlayer = GetVulnerableTarget(info, 3);
                if (targetPlayer == null)
                    return;
                _manager.InitiateLastRequest(fromPlayer.First(), targetPlayer.First(), type.Value);
                break;
            }
        }
    }
}