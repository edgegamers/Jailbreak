using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Mod.Rebel;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Debug.Subcommands;

// css_markrebel [player] <duration>
public class Pardon : AbstractCommand
{
    public Pardon(IServiceProvider services) : base(services)
    {
    }

    public override void OnCommand(CCSPlayerController? executor, WrappedInfo info)
    {
        if (info.ArgCount == 1)
        {
            info.ReplyToCommand("Specify target?");
            return;
        }

        var target = GetVulnerableTarget(info);
        if (target == null)
            return;
        
        foreach (var player in target.Players)
        {
            services.GetRequiredService<IRebelService>().UnmarkRebel(player);
        }

        info.ReplyToCommand($"Pardoned {GetTargetLabel(info)}");
    }
}