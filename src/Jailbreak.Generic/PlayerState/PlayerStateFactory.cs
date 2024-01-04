using Jailbreak.Generic.PlayerState.Behaviors;
using Jailbreak.Public.Generic;

namespace Jailbreak.Generic.PlayerState;

public class PlayerStateFactory : IPlayerStateFactory
{
	private readonly AliveStateTracker _alive;
	private readonly GlobalStateTracker _global;
	private readonly RoundStateTracker _round;

	public PlayerStateFactory(GlobalStateTracker global, AliveStateTracker alive, RoundStateTracker round)
	{
		_global = global;
		_alive = alive;
		_round = round;
	}

	public IPlayerState<T> Global<T>() where T : class, new()
	{
		var state = new PlayerStateImpl<T>();

		_global.Track(state);

		return state;
	}

	public IPlayerState<T> Alive<T>() where T : class, new()
	{
		var state = new PlayerStateImpl<T>();

		_global.Track(state);
		_alive.Track(state);

		return state;
	}

	public IPlayerState<T> Round<T>() where T : class, new()
	{
		var state = new PlayerStateImpl<T>();

		_global.Track(state);
		_round.Track(state);

		return state;
	}
}
