using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Jailbreak.Formatting.Views.Logging;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;

namespace Jailbreak.Logs.Listeners;

public class LogDamageListeners : IPluginBehavior {
  private readonly IRichLogService logs;

  public LogDamageListeners(IRichLogService logs) { this.logs = logs; }

  [GameEventHandler]
  public HookResult OnGrenadeThrown(EventGrenadeThrown @event,
    GameEventInfo info) {
    var player = @event.Userid;
    if (player == null || !player.IsReal()) return HookResult.Continue;
    var grenade = @event.Weapon;

    logs.Append(logs.Player(player), $"threw a {grenade}");

    return HookResult.Continue;
  }

  [GameEventHandler]
  public HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info) {
    var player = @event.Userid;
    if (player == null || !player.IsReal()) return HookResult.Continue;
    var attacker = @event.Attacker;

    var isWorld = attacker == null || !attacker.IsReal();
    var health  = @event.DmgHealth;

    if (isWorld) {
      if (health > 0)
        logs.Append("The world hurt", logs.Player(player),
          $"for {health} damage");
      else
        logs.Append("The world killed", logs.Player(player));
    } else {
      if (attacker == null) {
        logs.Append(logs.Player(player), "was hurt by an unknown source",
          $"for {health} damage");
        return HookResult.Continue;
      }

      if (health > 0)
        logs.Append(logs.Player(attacker), "hurt", logs.Player(player),
          $"for {health} damage");
      else
        logs.Append(logs.Player(attacker), "killed", logs.Player(player));
    }

    return HookResult.Continue;
  }
}