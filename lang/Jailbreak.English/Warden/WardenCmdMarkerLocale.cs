using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views.Warden;

namespace Jailbreak.English.Warden;

public class WardenCmdMarkerLocale : IWardenCmdMarkerLocale,
  ILanguage<Formatting.Languages.English> {
  public IView ChangingNotEnabled
    => new SimpleView {
      WardenLocale.PREFIX, "Marker customization is not supported on this server."
    };

  public IView TypeChanged(string type) {
    return new SimpleView {
      WardenLocale.PREFIX, $"Changed marker type to {type}."
    };
  }

  public IView ColorChanged(string color) {
    return new SimpleView {
      WardenLocale.PREFIX, $"Changed marker color to {color}."
    };
  }
}