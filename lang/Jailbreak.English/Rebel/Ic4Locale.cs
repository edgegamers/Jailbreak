using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.Rebel;

public class Ic4Locale : IC4Locale, ILanguage<Formatting.Languages.English> {
  public IView JihadC4Pickup
    => new SimpleView { RebelLocale.PREFIX, "You picked up a Jihad C4!" };

  public IView JihadC4Received
    => new SimpleView { RebelLocale.PREFIX, "You received a Jihad C4!" };

  public IView JihadC4Usage1
    => new SimpleView {
      RebelLocale.PREFIX,
      $"To detonate it, hold it out and press {ChatColors.Yellow + "E" + ChatColors.Default}."
    };
}