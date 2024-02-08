using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Mod.Draw;

public interface ILine : IDrawable, IColorable
{
	/// <summary>
	/// The second position that makes up this line.
	/// The line will terminate here.
	/// </summary>
	Vector EndPosition { get; }
}
