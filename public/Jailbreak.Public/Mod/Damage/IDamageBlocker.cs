using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Extensions;

namespace Jailbreak.Public.Mod.Damage;

/// <summary>
///   Wrapper for managing a player's health and preventing a player from
///   taking damage.
/// </summary>
public interface IDamageBlocker {
  [Obsolete("Do not use the EventPlayerHurt overload.")]
  bool ShouldBlockDamage(CCSPlayerController victim,
    CCSPlayerController? attacker, EventPlayerHurt @event) {
    return ShouldBlockDamage(victim, attacker);
  }

  bool ShouldBlockDamage(CCSPlayerController victim,
    CCSPlayerController? attacker);
}