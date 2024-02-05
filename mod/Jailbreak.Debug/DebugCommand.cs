using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using Jailbreak.Public.Behaviors;

namespace Jailbreak.Debug;

public class DebugCommand : IPluginBehavior
{
    [RequiresPermissions("@css/root")]
    public void Command_Debug(CCSPlayerController? executor, CommandInfo info)
    {
        if (executor == null)
        {
            return;
        }

        if (info.ArgCount == 1)
        {
        }
    }
}