using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Generic;

namespace Jailbreak.Generic.PlayerState;

public class PlayerStateImpl<TState> : IPlayerState<TState>, ITrackedPlayerState
  where TState : class, new() {
  private readonly Dictionary<int, TState> states = new();

  public TState Get(CCSPlayerController controller) {
    //	If the state doesn't exist, create it :^)
    states.TryAdd(controller.Slot, new TState());

    return states[controller.Slot];
  }

  public void Reset(CCSPlayerController controller) {
    states.Remove(controller.Slot);
  }

  public void Drop() { states.Clear(); }
}