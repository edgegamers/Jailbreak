using CounterStrikeSharp.API.Core;

namespace Jailbreak.Generic.PlayerState.Behaviors;

public class BaseStateTracker : IDisposable {
  private readonly List<ITrackedPlayerState> trackedPlayerStates = new();

  public void Dispose() { ResetAll(); }

  protected void Reset(CCSPlayerController controller) {
    foreach (var trackedPlayerState in trackedPlayerStates)
      trackedPlayerState.Reset(controller);
  }

  protected void ResetAll() {
    foreach (var trackedPlayerState in trackedPlayerStates)
      trackedPlayerState.Drop();
  }

  public void Track(ITrackedPlayerState state) {
    trackedPlayerStates.Add(state);
  }
}