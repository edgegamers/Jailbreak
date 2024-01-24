using CounterStrikeSharp.API.Core;

namespace Jailbreak.Generic.PlayerState.Behaviors;

public class BaseStateTracker : IDisposable
{
	private List<ITrackedPlayerState> _trackedPlayerStates = new();

	protected void Reset(CCSPlayerController controller)
	{
		foreach (ITrackedPlayerState trackedPlayerState in _trackedPlayerStates)
			trackedPlayerState.Reset(controller);
	}

	protected void ResetAll()
	{
		foreach (ITrackedPlayerState trackedPlayerState in _trackedPlayerStates)
			trackedPlayerState.Drop();
	}

	public void Track(ITrackedPlayerState state)
	{
		_trackedPlayerStates.Add(state);
	}

	public void Dispose()
		=> ResetAll();
}
