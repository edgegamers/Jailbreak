using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Views.RTD;

namespace Jailbreak.English.RTD;

public class AutoRTDLocale : IAutoRTDLocale {
  public IView TogglingNotEnabled
    => new SimpleView {
      RTDLocale.PREFIX, "Toggline Auto-RTD is not supported on this server."
    };

  public IView AutoRTDToggled(bool enabled) {
    return new SimpleView {
      RTDLocale.PREFIX,
      ChatColors.Grey + "You",
      enabled ? ChatColors.Green + "enabled" : ChatColors.Red + "disabled",
      ChatColors.Grey + "Auto-RTD."
    };
  }
}