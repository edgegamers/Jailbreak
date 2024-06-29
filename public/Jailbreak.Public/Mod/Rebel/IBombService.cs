using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.Rebel;

public interface IBombService
{

	/// <summary>
	/// Give the C4 to a player
	/// </summary>
	/// <param name="player"></param>
	/// <returns></returns>
	bool TryGrant(CCSPlayerController player);

}
