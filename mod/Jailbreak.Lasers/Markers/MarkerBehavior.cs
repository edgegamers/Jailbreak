using CounterStrikeSharp.API.Core;

using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Generic;
using Jailbreak.Public.Mod.Draw;

namespace Jailbreak.Drawable.Markers;

public class MarkerBehavior : IPluginBehavior, IMarkerService
{
	private IPlayerState<MarkerState> _state;

	public MarkerBehavior(IPlayerStateFactory playerStateFactory)
	{
		_state = playerStateFactory.Global<MarkerState>();
	}

	public bool TryEnable(CCSPlayerController player)
	{
		_state.Get(player)
			.Enabled = true;

		return true;
	}

	public bool TryDisable(CCSPlayerController player)
	{
		_state.Get(player)
			.Enabled = false;

		return true;
	}

	public bool IsEnabled(CCSPlayerController player)
	{
		return _state.Get(player)
			.Enabled;
	}
}
