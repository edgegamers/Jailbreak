using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;

using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Generic;
using Jailbreak.Public.Mod.Draw;

using NodaTime;

namespace Jailbreak.Drawable.Markers;

public class MarkerListener : IPluginBehavior
{
	public const int DEFAULT_SIZE = 150;

	private IMarkerService _markers;
	private IPlayerState<CurrentMarkerState> _current;
	private IDrawableFactory _drawable;

	public MarkerListener(IMarkerService markers, IPlayerStateFactory factory, IDrawableFactory drawable)
	{
		_markers = markers;
		_drawable = drawable;

		//	Use alive so the state tracker will dispose
		//	of the current state when the player dies.
		_current = factory.Alive<CurrentMarkerState>();
	}

	protected void HandlePing(CCSPlayerController player, CurrentMarkerState markerState, Vector position)
	{
		if (markerState.HasMarker
		    && markerState.CurrentMarker != null
		    && markerState.PlacedOn != null
		    && markerState.PlacedAt != null)
		{
			//	The player already has a marker down.
			//	Do we qualify for a resize operation?
			var elapsed = SystemClock.Instance.GetCurrentInstant() - markerState.PlacedOn;

			if (elapsed <= Duration.FromMilliseconds(750))
			{
				//	Resize the existing marker
				var distance = Math.Clamp(markerState.PlacedAt.Distance(position), 100, 500);
				markerState.CurrentMarker.SetRadius(distance);
				markerState.CurrentMarker.Draw();
				return;
			}
		}

		//	Create the marker if it does not exist
		markerState.HasMarker = true;
		markerState.CurrentMarker ??= _drawable.MarkerFor(player, position, DEFAULT_SIZE);
		markerState.CurrentMarker.SetPosition(position);
		markerState.CurrentMarker.Draw();

		markerState.PlacedAt = position;
		markerState.PlacedOn = SystemClock.Instance.GetCurrentInstant();
	}

	[GameEventHandler]
	public HookResult OnPing(EventPlayerPing ev, GameEventInfo info)
	{
		var player = ev.Userid;

		if (!_markers.IsEnabled(player))
			return HookResult.Handled;

		var vec = new Vector(ev.X, ev.Y, ev.Z);
		var current = _current.Get(player);

		this.HandlePing(player, current, vec);
		return HookResult.Handled;
	}
}
