using CounterStrikeSharp.API.Modules.Utils;

using Jailbreak.Public.Mod.Draw;

using NodaTime;

namespace Jailbreak.Drawable.Markers;

public class CurrentMarkerState : IDisposable
{
	public bool HasMarker { get; set; } = false;

	public IMarker? CurrentMarker { get; set; } = null;

	/// <summary>
	/// The time that the marker was placed.
	/// Used for resize operations, to avoid making every operation a resize.
	/// </summary>
	public Instant? PlacedOn { get; set; } = null;

	/// <summary>
	/// The position of the last ping, for resize purposes.
	/// </summary>
	public Vector? PlacedAt { get; set; } = null;

	/// <summary>
	/// Invoked when the player dies or leaves.
	/// </summary>
	public void Dispose()
	{
		if (CurrentMarker != null)
		{
			CurrentMarker.Remove();
			CurrentMarker.Dispose();
		}
	}
}
