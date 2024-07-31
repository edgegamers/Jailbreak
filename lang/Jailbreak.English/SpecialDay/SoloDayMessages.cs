using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Utils;

namespace Jailbreak.English.SpecialDay;

public class SoloDayMessages(string name, params string[] description)
  : ISpecialDayInstanceMessages {
  public string Name => name;

  public string[] Description
    => description.Select(s => s + SimpleView.NEWLINE).ToArray();

  public IView GenerateStartMessage() {
    if (Description.Length == 0) {
      return new SimpleView {
        SpecialDayMessages.PREFIX, "Today is a", Name, "day."
      };
    }

    if (Description.Length == 1) {
      return new SimpleView {
        { SpecialDayMessages.PREFIX, "Today is a", Name, "day." },
        SimpleView.NEWLINE,
        { SpecialDayMessages.PREFIX, Description[0] }
      };
    }

    return new SimpleView {
      { SpecialDayMessages.PREFIX, "Today is a", Name, "day." },
      SimpleView.NEWLINE,
      string.Join(SpecialDayMessages.PREFIX.ToChat(), Description)
    };
  }

  public virtual IView SpecialDayStart => GenerateStartMessage();

  IView ISpecialDayInstanceMessages.SpecialDayEnd
    => new SimpleView { SpecialDayMessages.PREFIX, Name, "ended." };

  public virtual IView BeginsIn(int seconds) {
    return seconds == 0 ?
      new SimpleView { SpecialDayMessages.PREFIX, Name, "begins now!" } :
      new SimpleView {
        SpecialDayMessages.PREFIX,
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
        SpecialDayMessages.PREFIX, Name, "has ended! No one won!"
      };
    return new SimpleView {
      SpecialDayMessages.PREFIX,
      Name,
      "ended!",
      lastAlive,
      "won!"
    };
  }
}