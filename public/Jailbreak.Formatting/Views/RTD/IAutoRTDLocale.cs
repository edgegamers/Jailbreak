using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views.RTD;

public interface IAutoRTDLocale {
  public IView TogglingNotEnabled();
  public IView AutoRTDToggled(bool enabled);
}