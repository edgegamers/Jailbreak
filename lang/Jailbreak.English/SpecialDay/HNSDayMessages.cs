using Jailbreak.Formatting.Base;

namespace Jailbreak.English.SpecialDay;

public class HNSDayMessages() : TeamDayMessages("Hide and Seek",
  "CTs must hide while the Ts seek!", "Ts have 250 HP!") {
  public IView StayInArmory
    => new SimpleView {
      SpecialDayMessages.PREFIX, "Today is", Name, ", stay in the armory!"
    };

  public override IView BeginsIn(int seconds) {
    if (seconds == 0)
      return new SimpleView {
        SpecialDayMessages.PREFIX, "Ready or not, here they come!"
      };
    return new SimpleView {
      SpecialDayMessages.PREFIX,
      Name,
      "begins in",
      seconds,
      "seconds."
    };
  }

  public IView DamageWarning(int seconds) {
    return new SimpleView {
      SpecialDayMessages.PREFIX,
      "You will be vulnerable to damage in",
      seconds,
      "seconds."
    };
  }
}