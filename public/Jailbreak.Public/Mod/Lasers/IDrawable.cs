
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Mod.Draw;

public interface IDrawable : IDisposable
{

	/// <summary>
	/// The position of the drawable object
	/// </summary>
	Vector Position { get; }

	void SetPosition(Vector position);

	/// <summary>
	/// Has this object been written to the world?
	/// </summary>
	bool Drawn { get; }

	/// <summary>
	/// Draw this item
	/// The drawable should remove itself if it is currently drawn
	/// </summary>
	void Draw();

	/// <summary>
	/// Remove this item
	/// </summary>
	void Remove();

	void IDisposable.Dispose()
	{
		this.Remove();
	}
}
