using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using Jailbreak.Debug.Subcommands;
using Jailbreak.Public.Behaviors;

namespace Jailbreak.Debug;

// css_debug [subcommand] [args] -> subcommand [args]
public class DebugCommand : IPluginBehavior
{

    private readonly Dictionary<string, Executor> commands = new();
    
    public DebugCommand()
    {
        commands.Add("markrebel", new MarkRebel());
    }
    
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