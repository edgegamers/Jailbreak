using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Utils;

namespace Jailbreak.English.SpecialDay;

public class SoloDayMessages(string name, string? description = null)
  : ISpecialDayInstanceMessages {
  public string Name => name;
  public string? Description => description;

  public virtual IView SpecialDayStart
    => Description == null ?
      new SimpleView {
        ISpecialDayMessages.PREFIX, "Today is a", Name, "day."
      } :
      new SimpleView {
        ISpecialDayMessages.PREFIX,
        "Today is a",
        Name,
        "day.",
        SimpleView.NEWLINE,
        ISpecialDayMessages.PREFIX,
        Description
      };

  public virtual IView BeginsIn(int seconds) {
    return seconds == 0 ?
      new SimpleView { ISpecialDayMessages.PREFIX, Name, "begins now!" } :
      new SimpleView {
        ISpecialDayMessages.PREFIX,
        Name,
        "begins in",
        seconds,
        "seconds."
      };
  }

  public IView SpecialDayEnd() {
    var lastAlive = PlayerUtil.GetAlive().FirstOrDefault();
    if (lastAlive == null)
      return new SimpleView {
        ISpecialDayMessages.PREFIX, Name, "has ended! No one won!"
      };
    return new SimpleView {
      ISpecialDayMessages.PREFIX,
      Name,
      "ended!",
      lastAlive,
      "won!"
    };
  }
}