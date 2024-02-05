using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Extensions;

public static class PlayerExtensions
{
	public static CsTeam GetTeam(this CCSPlayerController controller)
	{
		return (CsTeam)controller.TeamNum;
	}

	public static bool IsReal(this CCSPlayerController player)
	{
		//  Do nothing else before this:
		//  Verifies the handle points to an entity within the global entity list.
		if (!player.IsValid)
			return false;
        
		if(player.Connected != PlayerConnectedState.PlayerConnected)
			return false;

		if (player.IsBot || player.IsHLTV)
			return false;

		return true;
	}

}
