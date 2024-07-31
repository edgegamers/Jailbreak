using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.SpecialDay;

public class SpecialDayMessages : ISpecialDayMessages,
  ILanguage<Formatting.Languages.English> {
  public IView SpecialDayRunning(string name) {
    return new SimpleView {
      ISpecialDayMessages.PREFIX, name, "is currently running!"
    };
  }

  public IView InvalidSpecialDay(string name) {
    return new SimpleView {
      ISpecialDayMessages.PREFIX, name, "is not a valid special day!"
    };
  }

  public IView SpecialDayCooldown(int rounds) {
    return new SimpleView {
      ISpecialDayMessages.PREFIX,
      "You must wait",
      rounds,
      "more rounds before starting a special day!"
    };
  }

  public IView TooLateForSpecialDay(int maxTime) {
    return new SimpleView {
      ISpecialDayMessages.PREFIX,
      "You must start a special day within",
      maxTime,
      "seconds of the round start!"
    };
  }
}