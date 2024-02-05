using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.Logs;

namespace Jailbreak.Logs;

public class LogsCommand : IPluginBehavior
{
    private readonly ILogService _logs;

    public LogsCommand(ILogService logs)
    {
        this._logs = logs;
    }

    [ConsoleCommand("css_logs")]
    [RequiresPermissionsOr("@css/ban", "@css/generic", "@css/kick")]
    public void Command_Logs(CCSPlayerController? executor, CommandInfo info)
    {
        _logs.PrintLogs(executor);
    }
}