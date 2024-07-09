using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastGuard;
using Jailbreak.Public.Mod.Rebel;
using Jailbreak.Public.Mod.Warden;
using Jailbreak.Rebel;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Debug.Subcommands;

// css_lastguard <target>
public class LastGuard(IServiceProvider services) : AbstractCommand(services)
{
    public override void OnCommand(CCSPlayerController? executor, WrappedInfo info)
    {
        var lgService = Services.GetRequiredService<ILastGuardService>();

        var target = Utilities.GetPlayers()
            .FirstOrDefault(p => p.IsReal() && p is { Team: CsTeam.CounterTerrorist, PawnIsAlive: true });

        if (info.ArgCount == 2)
        {
            var targetResult = GetVulnerableTarget(info);
            if (targetResult == null)
                return;
            target = targetResult.First();
        }

        if (target == null)
            return;

        lgService.StartLastGuard(target);
        info.ReplyToCommand("Enabled LastGuard for " + target.PlayerName);
    }
}