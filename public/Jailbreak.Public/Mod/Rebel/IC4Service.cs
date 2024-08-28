﻿using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.Rebel;

public interface IC4Service {
  /// <summary>
  ///   Tries to give the jihad C4 to the player, assuming the player object passed in is valid.
  /// </summary>
  /// <param name="player"></param>
  void TryGiveC4ToPlayer(CCSPlayerController player);

  /// <summary>
  ///   Tries to detonate the jihad c4, relative to the player's position and after the specified delay.
  /// </summary>
  /// <param name="player"></param>
  /// <param name="delay"></param>
  /// <param name="bombEntity"></param>
  void StartDetonationAttempt(CCSPlayerController player, float delay,
    CC4 bombEntity);

  /// <summary>
  ///   Attempts to give the Jihad C4 to a random Terrorist,
  ///   if they already have one and they are chosen then nothing will
  ///   happen.
  /// </summary>
  void TryGiveC4ToRandomTerrorist();

  void DontGiveC4NextRound() { }

  void ClearActiveC4s();
}