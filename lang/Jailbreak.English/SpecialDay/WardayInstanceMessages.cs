using Jailbreak.Formatting.Base;

namespace Jailbreak.English.SpecialDay;

public class WardayInstanceMessages() : TeamDayMessages("Warday",
  "CTs vs Ts! CTs pick a room, Ts must go fight them!",
  "CTs MUST stay in the same room until expansion time.") {
  public IView ExpandNow
    => new SimpleView { { SpecialDayMessages.PREFIX, "CTs can expand now!" } };

  public IView ExpandIn(int seconds) {
    return new SimpleView {
      { SpecialDayMessages.PREFIX, "CTs can expand in", seconds, "seconds." },
      SimpleView.NEWLINE, {
        SpecialDayMessages.PREFIX,
        "They will be given replenished health, armor, and speed."
      }
    };
  }
}