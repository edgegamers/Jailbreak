using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views.Warden;

public interface IWardenCmdSoccerLocale {
  public IView SoccerSpawned { get; }
  public IView SpawnFailed { get; }
  public IView TooManySoccers { get; }
}