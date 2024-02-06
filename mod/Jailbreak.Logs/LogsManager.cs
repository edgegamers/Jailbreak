using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;

using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Objects;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Logs;
using Jailbreak.Public.Mod.Rebel;
using Jailbreak.Public.Mod.Warden;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Logs;

public class LogsManager : IPluginBehavior, ILogService
{
    private readonly List<IView> _logMessages = new();

    private ILogMessages _messages;

    public LogsManager(IServiceProvider serviceProvider, ILogMessages messages)
    {
        _messages = messages;

    }

    [GameEventHandler]
    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        _messages.BEGIN_JAILBREAK_LOGS
            .ToServerConsole()
            .ToAllConsole();

        //  By default, print all logs to player consoles at the end of the round.
        foreach (var log in _logMessages)
            log.ToServerConsole()
                .ToAllConsole();

        _messages.END_JAILBREAK_LOGS
            .ToServerConsole()
            .ToAllConsole();

        return HookResult.Continue;
    }

    [GameEventHandler]
    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        Clear();
        return HookResult.Continue;
    }

    public void Append(params FormatObject[] objects)
    {
        _logMessages.Add(_messages.CREATE_LOG(objects));
    }

    public void Append(string message)
    {
        _logMessages.Add(_messages.CREATE_LOG(message));
    }

    public IEnumerable<string> GetMessages()
    {
        return _logMessages.SelectMany(view => view.ToWriter().Plain);
    }

    public void Clear()
    {
        _logMessages.Clear();
    }

    public void PrintLogs(CCSPlayerController? player)
    {
        if (player == null || !player.IsReal())
        {
            _messages.BEGIN_JAILBREAK_LOGS
                .ToServerConsole();
            foreach (var log in _logMessages)
                log.ToServerConsole();
            _messages.END_JAILBREAK_LOGS
                .ToServerConsole();

            return;
        }


        _messages.BEGIN_JAILBREAK_LOGS
            .ToPlayerConsole(player);
        foreach (var log in _logMessages)
            log.ToPlayerConsole(player);
        _messages.END_JAILBREAK_LOGS
            .ToPlayerConsole(player);
    }
}
