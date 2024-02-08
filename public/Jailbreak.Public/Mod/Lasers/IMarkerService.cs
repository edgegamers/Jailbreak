using System.Drawing;

using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.Draw;

public interface IMarkerService
{

	/// <summary>
	/// Grant this player the ability to use a marker
	/// This persists until <see cref="TryDisable"/> is called.
	/// Succeeds if the player already has marker permissions.
	/// </summary>
	/// <param name="player"></param>
	/// <returns></returns>
	bool TryEnable(CCSPlayerController player);

	/// <summary>
	/// Try to remove marker permissions from this player.
	/// Succeeds if the player does not have marker permissions in the first place.
	/// </summary>
	/// <param name="player"></param>
	/// <returns></returns>
	bool TryDisable(CCSPlayerController player);

	/// <summary>
	/// Returns true if the specified player has marker privileges.
	/// </summary>
	/// <param name="player"></param>
	/// <returns></returns>
	bool IsEnabled(CCSPlayerController player);

}
