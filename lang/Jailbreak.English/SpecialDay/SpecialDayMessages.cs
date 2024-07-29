using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.SpecialDay;

public class SpecialDayMessages : ISpecialDayMessages,
  ILanguage<Formatting.Languages.English> {
  public IView SpecialDayRunning(string name)
    => new SimpleView {
      ISpecialDayMessages.PREFIX, name, "is currently running!"
    };

  public IView InvalidSpecialDay(string name)
    => new SimpleView {
      ISpecialDayMessages.PREFIX, name, "is not a valid special day!"
    };

  public IView SpecialDayCooldown(int rounds)
    => new SimpleView {
      ISpecialDayMessages.PREFIX,
      "You must wait",
      rounds,
      "more rounds before starting a special day!"
    };
}