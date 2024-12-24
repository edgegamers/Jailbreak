using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.Warden;

public interface ISpecialTreatmentService {
  public bool IsSpecialTreatment(CCSPlayerController player);

  public void SetSpecialTreatment(CCSPlayerController player, bool special) {
    if (special)
      Grant(player);
    else
      Revoke(player);
  }

  /// <summary>
  ///   Give this player ST for the rest of the round
  /// </summary>
  /// <param name="player"></param>
  public void Grant(CCSPlayerController player);

  /// <summary>
  ///   Revoke the player's special treatment for the current round
  ///   Does nothing if not ST.
  /// </summary>
  /// <param name="player"></param>
  public void Revoke(CCSPlayerController player) { Revoke(player, true); }

  /// <summary>
  ///   Revoke the player's special treatment for the current round
  ///   Does nothing if not ST.
  /// </summary>
  /// <param name="player"></param>
  ///  <param name="print">Whether to print a message to the player</param>
  public void Revoke(CCSPlayerController player, bool print);
}