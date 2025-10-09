using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.Damage;

/// <summary>
///   Wrapper for managing a player's health and preventing a player from
///   taking damage.
/// </summary>
public interface IDamageBlocker {
  bool ShouldBlockDamage(CCSPlayerController victim,
    CCSPlayerController? attacker);
}