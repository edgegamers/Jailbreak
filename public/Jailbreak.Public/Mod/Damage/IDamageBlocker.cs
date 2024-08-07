using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Extensions;

namespace Jailbreak.Public.Mod.Damage;

/// <summary>
/// Wrapper for managing a player's health and preventing a player from
/// taking damage.
/// </summary>
public interface IDamageBlocker {
  HookResult BlockUserDamage(EventPlayerHurt @event, GameEventInfo _) {
    var player   = @event.Userid;
    var attacker = @event.Attacker;
    if (player == null || !player.IsReal()) return HookResult.Continue;

    if (!ShouldBlockDamage(player, attacker, @event))
      return HookResult.Continue;
    if (player.PlayerPawn.IsValid) {
      var playerPawn = player.PlayerPawn.Value!;
      playerPawn.Health = playerPawn.LastHealth;
    }

    @event.DmgArmor  = 0;
    @event.DmgHealth = 0;
    return HookResult.Stop;
  }

  bool ShouldBlockDamage(CCSPlayerController victim,
    CCSPlayerController? attacker, EventPlayerHurt @event);
}