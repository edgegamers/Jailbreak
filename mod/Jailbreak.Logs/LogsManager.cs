using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Logs;
using Jailbreak.Public.Mod.Rebel;
using Jailbreak.Public.Mod.Warden;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Logs;

public class LogsManager : IPluginBehavior, ILogService
{
    private readonly List<string> _logMessages = new();
    private long startTime;
    private IWardenService wardenService;
    private IRebelService rebelService;

    private IServiceProvider _serviceProvider;

    public LogsManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Start(BasePlugin parent)
    {
        parent.RegisterEventHandler<EventRoundStart>(OnRoundStart);
        parent.RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        wardenService = _serviceProvider.GetRequiredService<IWardenService>();
        rebelService = _serviceProvider.GetRequiredService<IRebelService>();
    }

    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        foreach (var player in Utilities.GetPlayers())
        {
            if (!player.IsValid || player.IsBot || player.IsHLTV)
                continue;
            foreach (var log in _logMessages)
            {
                player.PrintToConsole(log);
            }
        }

        return HookResult.Continue;
    }

    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        startTime = DateTimeOffset.Now.ToUnixTimeSeconds();
        ClearLogMessages();
        return HookResult.Continue;
    }

    public void AddLogMessage(string message)
    {
        // format to [MM:SS] message
        string prefix = $"[{TimeSpan.FromSeconds(DateTimeOffset.Now.ToUnixTimeSeconds() - startTime):mm\\:ss}] ";
        _logMessages.Add(prefix + message);
    }

    public ICollection<string> GetLogMessages()
    {
        return _logMessages;
    }

    public void ClearLogMessages()
    {
        _logMessages.Clear();
    }

    public string FormatPlayer(CCSPlayerController player)
    {
        if (wardenService.IsWarden(player))
            return $"{player.PlayerName} (WARDEN)";
        if (player.GetTeam() == CsTeam.CounterTerrorist)
            return $"{player.PlayerName} (CT)";
        if (rebelService.IsRebel(player))
            return $"{player.PlayerName} (REBEL)";
        return $"{player.PlayerName} (Prisoner)";
    }


    public void PrintLogs(CCSPlayerController? player)
    {
        if (player == null)
        {
            printLogs(Server.PrintToConsole);
        }
        else if (player.IsValid && !player.IsBot && !player.IsHLTV)
        {
            printLogs(player.PrintToConsole);
        }
    }

    private void printLogs(Delegate printFunction)
    {
        if (!GetLogMessages().Any())
        {
            printFunction.DynamicInvoke("No logs to display.");
            return;
        }

        printFunction.DynamicInvoke("********************************");
        printFunction.DynamicInvoke("***** BEGIN JAILBREAK LOGS *****");
        printFunction.DynamicInvoke("********************************");
        foreach (string log in GetLogMessages())
        {
            printFunction.DynamicInvoke(log);
        }

        printFunction.DynamicInvoke("********************************");
        printFunction.DynamicInvoke("****** END JAILBREAK LOGS ******");
        printFunction.DynamicInvoke("********************************");
    }
}