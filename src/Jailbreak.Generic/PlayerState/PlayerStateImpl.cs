using CounterStrikeSharp.API.Core;

using Jailbreak.Public.Generic;

namespace Jailbreak.Generic.PlayerState;

public class PlayerStateImpl<TState> : IPlayerState<TState>, ITrackedPlayerState
	where TState : class, new()
{
	private Dictionary<int, TState> _states = new();

	public TState Get(CCSPlayerController controller)
	{
		//	If the state doesn't exist, create it :^)
		_states.TryAdd(controller.Slot, new TState());

		return _states[controller.Slot];
	}

	public void Reset(CCSPlayerController controller)
		=> _states.Remove(controller.Slot);

	public void Drop()
		=> _states.Clear();
}
