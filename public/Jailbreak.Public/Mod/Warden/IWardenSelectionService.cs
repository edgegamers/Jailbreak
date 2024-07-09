using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.Warden;

public interface IWardenSelectionService {
  /// <summary>
  ///   Whether or not the warden queue is currently in use
  /// </summary>
  bool Active { get; }

  /// <summary>
  ///   Enter this player into the warden queue
  /// </summary>
  /// <param name="player"></param>
  bool TryEnter(CCSPlayerController player);

  /// <summary>
  ///   Remove this player from the warden queue
  /// </summary>
  /// <param name="player"></param>
  bool TryExit(CCSPlayerController player);

  /// <summary>
  ///   Determine whether this player is in the queue or not
  /// </summary>
  /// <param name="player"></param>
  /// <returns></returns>
  bool InQueue(CCSPlayerController player);
}