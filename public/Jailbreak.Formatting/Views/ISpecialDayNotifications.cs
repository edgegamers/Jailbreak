using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views;

public interface ISpecialDayNotifications {
  public IView SdWardayStarted { get; }
  public IView SdFreedayStarted { get; }
  public IView SdFfaStarting { get; }
  public IView SdFfaStarted { get; }
  public IView SdCustomStarted { get; }
  public IView SdCantStart { get; }
  public IView SdNotWarden { get; }
}