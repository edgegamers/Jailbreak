using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.Logs;

public interface IPlayerTag
{
	/// <summary>
	/// Get a tag that contains context about the player.
	/// </summary>
	/// <param name="playerController"></param>
	/// <returns></returns>
	string Plain(CCSPlayerController playerController);
}
