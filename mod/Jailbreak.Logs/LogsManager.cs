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

    private readonly IServiceProvider _serviceProvider;
    private IRebelService? _rebelService;
    private long _startTime;
    private IWardenService? _wardenService;

    public LogsManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void AddLogMessage(string message)
    {
        // format to [MM:SS] message
        var prefix = $"[{TimeSpan.FromSeconds(DateTimeOffset.Now.ToUnixTimeSeconds() - _startTime):mm\\:ss}] ";
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
        if(_rebelService == null || _wardenService == null)
            throw new InvalidOperationException("Services not initialized");
        if (_wardenService.IsWarden(player))
            return $"{player.PlayerName} (WARDEN)";
        if (player.GetTeam() == CsTeam.CounterTerrorist)
            return $"{player.PlayerName} (CT)";
        if (_rebelService.IsRebel(player))
            return $"{player.PlayerName} (REBEL)";
        return $"{player.PlayerName} (Prisoner)";
    }


    public void PrintLogs(CCSPlayerController? player)
    {
        if (player == null)
            PrintLogs(Server.PrintToConsole);
        else if (player.IsReal()) PrintLogs(player.PrintToConsole);
    }

    public void Start(BasePlugin parent)
    {
        parent.RegisterEventHandler<EventRoundStart>(OnRoundStart);
        parent.RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        _wardenService = _serviceProvider.GetRequiredService<IWardenService>();
        _rebelService = _serviceProvider.GetRequiredService<IRebelService>();
    }

    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        foreach (var player in Utilities.GetPlayers())
        {
            if (!player.IsReal())
                continue;
            foreach (var log in _logMessages) player.PrintToConsole(log);
        }

        return HookResult.Continue;
    }

    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        _startTime = DateTimeOffset.Now.ToUnixTimeSeconds();
        ClearLogMessages();
        return HookResult.Continue;
    }

    private void PrintLogs(Delegate printFunction)
    {
        if (!GetLogMessages().Any())
        {
            printFunction.DynamicInvoke("No logs to display.");
            return;
        }

        printFunction.DynamicInvoke("********************************");
        printFunction.DynamicInvoke("***** BEGIN JAILBREAK LOGS *****");
        printFunction.DynamicInvoke("********************************");
        foreach (var log in GetLogMessages()) printFunction.DynamicInvoke(log);

        printFunction.DynamicInvoke("********************************");
        printFunction.DynamicInvoke("****** END JAILBREAK LOGS ******");
        printFunction.DynamicInvoke("********************************");
    }
}