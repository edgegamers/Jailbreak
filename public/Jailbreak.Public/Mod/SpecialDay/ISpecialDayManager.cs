using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.SpecialDay.Enums;

namespace Jailbreak.Public.Mod.SpecialDay;

public interface ISpecialDayManager : IPluginBehavior {
  public bool IsSDRunning { get; set; }
  public AbstractSpecialDay? CurrentSD { get; }
  public int RoundsSinceLastSD { get; }

  /// <summary>
  ///   Function to check if the player can start the specified special day,
  ///   returns a string explanation if the player cannot start the special day.
  ///   Returns null if the player can start the special day.
  /// </summary>
  /// <param name="type"></param>
  /// <param name="player"></param>
  /// <returns></returns>
  string? CanStartSpecialDay(SDType type, CCSPlayerController? player);

  bool InitiateSpecialDay(SDType type);
}