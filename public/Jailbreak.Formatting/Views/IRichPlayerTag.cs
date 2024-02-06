using CounterStrikeSharp.API.Core;

using Jailbreak.Formatting.Core;

namespace Jailbreak.Formatting.Views;

public interface IRichPlayerTag
{
	/// <summary>
	/// Get a tag for this player, which contains context about the player's current actions
	/// </summary>
	/// <param name="player"></param>
	/// <returns></returns>
	FormatObject Rich(CCSPlayerController player);
}
