using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.Rebel;

public class C4Locale : IC4Locale, ILanguage<Formatting.Languages.English> {
  public IView JihadC4Pickup
    => new SimpleView {
      RebelLocale.PREFIX,
      $"You picked up a {ChatColors.Red}Jihad C4{ChatColors.Grey}!"
    };

  public IView JihadC4Received
    => new SimpleView {
      RebelLocale.PREFIX,
      $"You received a {ChatColors.Red}Jihad C4{ChatColors.Grey}!"
    };

  public IView JihadC4Usage1
    => new SimpleView {
      RebelLocale.PREFIX,
      $"To detonate it, hold it out and press {ChatColors.Yellow + "E" + ChatColors.Grey}."
    };
}