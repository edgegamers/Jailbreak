using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Generic;

namespace Jailbreak.Generic.PlayerState;

public class PlayerStateImpl<TState> : IPlayerState<TState>, ITrackedPlayerState
    where TState : class, new()
{
    private readonly Dictionary<int, TState> _states = new();

    public TState Get(CCSPlayerController controller)
    {
        //	If the state doesn't exist, create it :^)
        _states.TryAdd(controller.Slot, new TState());

        return _states[controller.Slot];
    }

    public void Reset(CCSPlayerController controller)
    {
        ResetInternal(controller.Slot);
    }

    public void Drop()
    {
        foreach (var (key, _) in _states)
            ResetInternal(key);
    }

    private void ResetInternal(int slot)
    {
        var entry = _states[slot];

        //  If the state is disposable,
        //  give the plugin a nice clean place to cleanup the state here.
        if (entry is IDisposable disposable)
            disposable.Dispose();

        _states.Remove(slot);
    }
}
