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
    private ILastRequestManager manager;
    private LastRequestPlayerSelector playerSelector;
    private LastRequestMenuSelector menuSelector;
    private ILastRequestMessages _messages;
    private IGenericCommandNotifications _generic;

    private BasePlugin plugin;

    public LastRequest(IServiceProvider services, BasePlugin plugin) : base(services)
    {
        this.plugin = plugin;
        manager = services.GetRequiredService<ILastRequestManager>();
        playerSelector = new LastRequestPlayerSelector(manager, true);
        menuSelector = new LastRequestMenuSelector(services.GetRequiredService<ILastRequestFactory>(),
            (type) => "css_debug lastrequest " + type);
        _messages = services.GetRequiredService<ILastRequestMessages>();
        _generic = services.GetRequiredService<IGenericCommandNotifications>();
    }

    // css_lastrequest [lr] [player] <target>
    public override void OnCommand(CCSPlayerController? executor, WrappedInfo info)
    {
        if (executor != null && !executor.IsReal())
            return;

        if (info.ArgCount == 1 && executor != null)
        {
            MenuManager.OpenCenterHtmlMenu(plugin, executor, menuSelector.GetMenu());
        }

        var type = LRTypeExtensions.FromString(info.GetArg(1));
        if (type is null)
        {
            _messages.InvalidLastRequest(info.GetArg(1)).ToPlayerChat(executor);
            return;
        }

        if (info.ArgCount == 2)
        {
            MenuManager.OpenCenterHtmlMenu(plugin, executor,
                playerSelector.CreateMenu(executor, (str) => "css_debug lastrequest " + type + " #" + str));
            return;
        }

        var fromPlayer = GetVulnerableTarget(info, 2);
        if (fromPlayer == null)
            return;

        if (info.ArgCount == 3 && executor != null)
        {
            if (executor.Team == CsTeam.Terrorist)
                manager.InitiateLastRequest(executor, fromPlayer.First(), type.Value);
            else // They aren't necessarily on different teams, but this is debug so that's OK
                manager.InitiateLastRequest(fromPlayer.First(), executor, type.Value);
            return;
        }

        if (info.ArgCount == 4)
        {
            var targetPlayer = GetVulnerableTarget(info, 3);
            if (targetPlayer == null)
                return;
            manager.InitiateLastRequest(fromPlayer.First(), targetPlayer.First(), type.Value);
        }
    }
}