using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using Jailbreak.Debug.Subcommands;
using Jailbreak.Public.Behaviors;

namespace Jailbreak.Debug;

// css_debug [subcommand] [args] -> subcommand [args]
public class DebugCommand : IPluginBehavior
{
    private readonly Dictionary<string, AbstractCommand> _commands = new();

    public DebugCommand(IServiceProvider serviceProvider)
    {
        _commands.Add("markrebel", new MarkRebel(serviceProvider));
        _commands.Add("pardon", new Pardon(serviceProvider));
    }

    [RequiresPermissions("@css/root")]
    [ConsoleCommand("css_debug", "Debug command for Jailbreak.")]
    public void Command_Debug(CCSPlayerController? executor, CommandInfo info)
    {
        if (executor == null) return;

        if (info.ArgCount == 1)
        {
            foreach (var command in _commands) info.ReplyToCommand(command.Key);

            return;
        }

        if (!_commands.TryGetValue(info.GetArg(1), out var subcommand))
        {
            info.ReplyToCommand("Invalid subcommand");
            return;
        }

        subcommand.OnCommand(executor, new WrappedInfo(info));
    }
}