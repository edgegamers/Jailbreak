using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Core;
using Jailbreak.Public.Mod.Logs;

namespace Jailbreak.Formatting.Views.Logging;

public interface IRichPlayerTag : IPlayerTag {
  /// <summary>
  ///   Get a tag for this player, which contains context about the player's current actions
  /// </summary>
  /// <param name="player"></param>
  /// <returns></returns>
  FormatObject Rich(CCSPlayerController player);
}