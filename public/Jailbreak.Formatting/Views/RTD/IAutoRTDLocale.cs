using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views.RTD;

public interface IAutoRTDLocale {
  public IView TogglingNotEnabled { get; }
  public IView AutoRTDToggled(bool enabled);
}