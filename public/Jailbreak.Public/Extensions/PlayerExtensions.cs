using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Extensions;

public static class PlayerExtensions
{
	public static CsTeam GetTeam(this CCSPlayerController controller)
	{
		return (CsTeam)controller.TeamNum;
	}
}