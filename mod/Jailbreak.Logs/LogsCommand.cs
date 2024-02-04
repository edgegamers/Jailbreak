using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.Logs;

namespace Jailbreak.Logs;

public class LogsCommand : IPluginBehavior
{
    private ILogService logs;

    public LogsCommand(ILogService logs)
    {
        this.logs = logs;
    }

    [ConsoleCommand("css_logs")]
    public void Command_Logs(CCSPlayerController? executor, CommandInfo info)
    {
        if (executor == null)
            return;

        foreach (var log in logs.GetLogMessages())
        {
            executor.PrintToConsole(log);
        }
    }
}