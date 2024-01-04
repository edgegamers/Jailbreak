using Jailbreak.Generic.Behaviors;
using Jailbreak.Public.Generic;

namespace Jailbreak.Generic;

public class PlayerStateFactory : IPlayerStateFactory
{
	private GlobalStateTracker _global;
	private AliveStateTracker _alive;
	private RoundStateTracker _round;

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

		return state;	}

	public IPlayerState<T> Round<T>() where T : class, new()
	{
		var state = new PlayerStateImpl<T>();

		_global.Track(state);
		_round.Track(state);

		return state;
	}
}
