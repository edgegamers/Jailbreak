using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.SpecialDay;

public class HNSInstanceMessages() : TeamDayMessages("Hide and Seek") {
  public override IView SpecialDayStart
    => new SimpleView {
      {
        ISpecialDayMessages.PREFIX, Name,
        "has begun! CTs must hide and Ts must seek!"
      },
      SimpleView.NEWLINE,
      { ISpecialDayMessages.PREFIX, "Ts have 250 HP!" }
    };

  public IView StayInArmory
    => new SimpleView {
      ISpecialDayMessages.PREFIX, "Today is", Name, ", stay in the armory!"
    };

  public IView ReadyOrNot
    => new SimpleView {
      ISpecialDayMessages.PREFIX, "Ready or not, here they come!"
    };
}