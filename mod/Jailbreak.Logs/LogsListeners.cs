using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.Logs;

namespace Jailbreak.Logs;

public class LogsListeners : IPluginBehavior
{
    private ILogService logs;

    public LogsListeners(ILogService logs)
    {
        this.logs = logs;
    }

    public void Start(BasePlugin parent)
    {
        parent.RegisterEventHandler<EventPlayerHurt>(OnPlayerHurt);
    }

    private HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!player.IsValid)
            return HookResult.Continue;
        var attacker = @event.Attacker;

        bool isWorld = attacker == null || !attacker.IsValid;
        int health = @event.Health;

        if (isWorld)
        {
            if (health > 0)
            {
                logs.AddLogMessage($"The world hurt {logs.FormatPlayer(player)} for {health} damage");
            }
            else
            {
                logs.AddLogMessage($"The world killed {logs.FormatPlayer(player)}");
            }
        }
        else
        {
            if (health > 0)
            {
                logs.AddLogMessage(
                    $"{logs.FormatPlayer(attacker!)} hurt {logs.FormatPlayer(player)} for {health} damage");
            }
            else
            {
                logs.AddLogMessage($"{logs.FormatPlayer(attacker!)} killed {logs.FormatPlayer(player)}");
            }
        }

        return HookResult.Continue;
    }
}