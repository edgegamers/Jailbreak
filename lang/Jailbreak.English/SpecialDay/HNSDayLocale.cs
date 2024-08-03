using Jailbreak.Formatting.Base;

namespace Jailbreak.English.SpecialDay;

public class HNSDayLocale() : TeamDayLocale("Hide and Seek",
  "CTs must hide while the Ts seek!", "Ts have 250 HP!") {
  public IView StayInArmory
    => new SimpleView { PREFIX, "Today is", Name, ", stay in the armory!" };

  public override IView BeginsIn(int seconds) {
    if (seconds == 0)
      return new SimpleView { PREFIX, "Ready or not, here they come!" };
    return new SimpleView {
      PREFIX,
      Name,
      "begins in",
      seconds,
      "seconds."
    };
  }

  public IView DamageWarning(int seconds) {
    return new SimpleView {
      PREFIX, "You will be vulnerable to damage in", seconds, "seconds."
    };
  }
}