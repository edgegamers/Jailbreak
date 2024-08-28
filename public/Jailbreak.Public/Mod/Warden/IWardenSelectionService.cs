using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.Warden;

public interface IWardenSelectionService {
  /// <summary>
  ///   Whether or not the warden queue is currently in use
  /// </summary>
  bool Active { get; }

  /// <summary>
  ///   Whether the player should be guaranteed warden if they enter the queue
  ///   this will act as a bypass to the queue. Thus, if multiple players are
  ///   applied with this, the first player to be apply for warden will receive it.
  /// </summary>
  /// <param name="player"></param>
  void SetGuaranteedWarden(CCSPlayerController player);

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