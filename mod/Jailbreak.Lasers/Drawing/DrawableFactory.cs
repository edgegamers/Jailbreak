using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Mod.Draw;

public class DrawableFactory : IDrawableFactory
{
	public ILine LineFor(CCSPlayerController? player, Vector from, Vector to)
	{
		throw new NotImplementedException();
	}

	public IMarker MarkerFor(CCSPlayerController? player, Vector position, float radius)
	{
		throw new NotImplementedException();
	}
}
