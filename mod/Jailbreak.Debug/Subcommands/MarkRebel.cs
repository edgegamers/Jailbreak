using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Mod.Rebel;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Debug.Subcommands;

// css_markrebel [player] <duration>
public class MarkRebel : AbstractCommand
{
    public MarkRebel(IServiceProvider services) : base(services)
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

        var duration = 120;
        if(info.ArgCount == 3)
        {
            if (!int.TryParse(info.GetArg(2), out duration))
            {
                info.ReplyToCommand("Invalid duration");
                return;
            }
        } 
        
        foreach (var player in target.Players)
        {
            services.GetRequiredService<IRebelService>().MarkRebel(player, duration);
        }
        info.ReplyToCommand($"Marked {target.Players.Count()} players as rebels for {duration} seconds");
    }
}