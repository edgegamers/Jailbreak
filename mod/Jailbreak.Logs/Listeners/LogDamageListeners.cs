using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;

using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;

namespace Jailbreak.Logs.Listeners;

public class LogDamageListeners : IPluginBehavior
{
    private readonly IRichLogService _logs;

    public LogDamageListeners(IRichLogService logs)
    {
        _logs = logs;
    }



    [GameEventHandler]
    public HookResult OnGrenadeThrown(EventGrenadeThrown @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!player.IsReal())
            return HookResult.Continue;
        var grenade = @event.Weapon;

        _logs.Append(_logs.Player(player), $"threw a {grenade}");

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!player.IsReal())
            return HookResult.Continue;
        var attacker = @event.Attacker;

        bool isWorld = attacker == null || !attacker.IsReal();
        int health = @event.DmgHealth;

        if (isWorld)
        {
            if (health > 0)
            {
                _logs.Append($"The world hurt", _logs.Player(player), $"for {health} damage");
            }
            else
            {
                _logs.Append("The world killed", _logs.Player(player));
            }
        }
        else
        {
            if (health > 0)
            {
                _logs.Append( _logs.Player(attacker), "hurt", _logs.Player(player), $"for {health} damage");
            }
            else
            {
                _logs.Append(_logs.Player(attacker!), "killed", _logs.Player(player));
            }
        }

        return HookResult.Continue;
    }
}
