using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.LastRequest;

/// <summary>
/// Interface for Last Requests that require additional configuration
/// before Setup() is called
/// </summary>
public interface ILastRequestConfig {
  /// <summary>
  /// Opens a configuration menu for the prisoner to make choices
  /// </summary>
  /// <param name="prisoner">The prisoner choosing the LR</param>
  /// <param name="guard">The guard selected for the LR</param>
  /// <param name="onComplete">Callback to invoke when configuration is complete</param>
  void OpenConfigMenu(CCSPlayerController prisoner, 
    CCSPlayerController guard, 
    Action onComplete);
  
  /// <summary>
  /// Whether this LR requires configuration
  /// </summary>
  bool RequiresConfiguration { get; }
}
