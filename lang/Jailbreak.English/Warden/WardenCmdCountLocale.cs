using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views.Warden;

namespace Jailbreak.English.Warden;

public class WardenCmdCountLocale : IWardenCmdCountLocale,
  ILanguage<Formatting.Languages.English> {
  public IView PrisonersInMarker(int prisoners) {
    return new SimpleView {
      WardenLocale.PREFIX,
      ChatColors.Grey + "There " + (prisoners == 1 ? "is" : " are"),
      prisoners,
      ChatColors.Grey + "prisoner" + (prisoners == 1 ? "" : "s")
      + " in the marker."
    };
  }

  public IView CannotCountYet(int seconds) {
    return new SimpleView {
      WardenLocale.PREFIX,
      "You must wait",
      seconds,
      "seconds before counting prisoners."
    };
  }

  public IView NoMarkerSet
    => new SimpleView { WardenLocale.PREFIX, "No marker set." };
}