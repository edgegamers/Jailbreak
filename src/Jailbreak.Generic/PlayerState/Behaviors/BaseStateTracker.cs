using CounterStrikeSharp.API.Core;

namespace Jailbreak.Generic.PlayerState.Behaviors;

public class BaseStateTracker : IDisposable
{
    private readonly List<ITrackedPlayerState> _trackedPlayerStates = new();

    public void Dispose()
    {
        ResetAll();
    }

    protected void Reset(CCSPlayerController controller)
    {
        foreach (var trackedPlayerState in _trackedPlayerStates)
            trackedPlayerState.Reset(controller);
    }

    protected void ResetAll()
    {
        foreach (var trackedPlayerState in _trackedPlayerStates)
            trackedPlayerState.Drop();
    }

    public void Track(ITrackedPlayerState state)
    {
        _trackedPlayerStates.Add(state);
    }
}