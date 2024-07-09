using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.LastRequest;

public class EndRaceCommand(ILastRequestManager lrManager) : IPluginBehavior
{
    [ConsoleCommand("css_endrace", "Used to set the end point of a race LR")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void Command_EndRace(CCSPlayerController? executor, CommandInfo info)
    {
        if (executor == null)
            return;
        var lr = lrManager.GetActiveLR(executor);

        if (lr is not { type: LRType.Race })
        {
            info.ReplyToCommand("You must be in a race LR to use this command.");
            return;
        }

        if (lr.state != LRState.Pending)
        {
            info.ReplyToCommand("You must be in the pending state to use this command.");
            return;
        }

        lr.Execute();
    }
}