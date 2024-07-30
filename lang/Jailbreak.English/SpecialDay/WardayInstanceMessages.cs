using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.SpecialDay;

public class WardayInstanceMessages() : TeamDayMessages("Warday",
  "CTs vs Ts! CTs pick a room, Ts must go fight them!") {
  public IView ExpandIn(int seconds)
    => new SimpleView {
      { ISpecialDayMessages.PREFIX, "CTs can expand in", seconds, "seconds." },
      SimpleView.NEWLINE, {
        ISpecialDayMessages.PREFIX,
        "They will be given replenished health, armor, and speed."
      }
    };

  public IView ExpandNow
    => new SimpleView { { ISpecialDayMessages.PREFIX, "CTs can expand now!" } };
}