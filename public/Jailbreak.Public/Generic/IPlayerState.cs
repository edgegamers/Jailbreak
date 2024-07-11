using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Generic;

/// <summary>
///   A player state dictionary that automatically deletes stale states
///   and translates between different client formats.
/// </summary>
/// <typeparam name="TState"></typeparam>
public interface IPlayerState<out TState> where TState : class, new() {
  TState Get(CCSPlayerController controller);
}