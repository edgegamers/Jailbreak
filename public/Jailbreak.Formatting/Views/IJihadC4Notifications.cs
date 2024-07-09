using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views;

public interface IJihadC4Notifications {
  public IView JihadC4Pickup { get; }
  public IView JihadC4Received { get; }

  public IView JihadC4Usage1 { get; }
}