using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views.Warden;

public interface IWardenCmdChickenLocale {
  public IView ChickenSpawned { get; }
  public IView SpawnFailed { get; }
  public IView TooManyChickens { get; }
}