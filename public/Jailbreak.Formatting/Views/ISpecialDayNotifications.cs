using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views;

public interface ISpecialDayNotifications {
  public IView SD_WARDAY_STARTED { get; }
  public IView SD_FREEDAY_STARTED { get; }
  public IView SD_FFA_STARTING { get; }
  public IView SD_FFA_STARTED { get; }
  public IView SD_CUSTOM_STARTED { get; }
  public IView SD_CANT_START { get; }
  public IView SD_NOT_WARDEN { get; }
}