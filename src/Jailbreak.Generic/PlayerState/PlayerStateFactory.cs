using Jailbreak.Generic.PlayerState.Behaviors;
using Jailbreak.Public.Generic;

namespace Jailbreak.Generic.PlayerState;

public class PlayerStateFactory(GlobalStateTracker global,
  AliveStateTracker alive, RoundStateTracker round) : IPlayerStateFactory {
  public IPlayerState<T> Global<T>() where T : class, new() {
    var state = new PlayerStateImpl<T>();

    global.Track(state);

    return state;
  }

  public IPlayerState<T> Alive<T>() where T : class, new() {
    var state = new PlayerStateImpl<T>();

    global.Track(state);
    alive.Track(state);

    return state;
  }

  public IPlayerState<T> Round<T>() where T : class, new() {
    var state = new PlayerStateImpl<T>();

    global.Track(state);
    round.Track(state);

    return state;
  }
}