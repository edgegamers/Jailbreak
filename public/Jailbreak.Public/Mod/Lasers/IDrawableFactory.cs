using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Mod.Draw;

public interface IDrawableFactory
{
	/// <summary>
	/// Draw a line beginning at from and ending at to.
	/// </summary>
	/// <param name="from"></param>
	/// <param name="to"></param>
	/// <returns></returns>
	ILine Line(Vector from, Vector to)
		=> this.LineFor(null, from, to);

	/// <summary>
	/// Draw a line beginning at from and ending at to,
	/// using the specified player's style configurations.
	/// </summary>
	/// <param name="player"></param>
	/// <param name="from"></param>
	/// <param name="to"></param>
	/// <returns></returns>
	ILine LineFor(CCSPlayerController? player, Vector from, Vector to);

	/// <summary>
	/// Draw a marker at the specified position with size "radius".
	/// </summary>
	/// <param name="position"></param>
	/// <param name="radius"></param>
	/// <returns></returns>
	IMarker Marker(Vector position, float radius)
		=> MarkerFor(null, position, radius);

	/// <summary>
	/// Draw a marker at the position with size radius,
	/// using the specified player's style configurations.
	/// </summary>
	/// <param name="player"></param>
	/// <param name="position"></param>
	/// <param name="radius"></param>
	/// <returns></returns>
	IMarker MarkerFor(CCSPlayerController? player, Vector position, float radius);
}
