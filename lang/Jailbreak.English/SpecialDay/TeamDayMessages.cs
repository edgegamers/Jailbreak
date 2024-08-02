using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Utils;

namespace Jailbreak.English.SpecialDay;

public class TeamDayMessages(string name, params string[] description)
  : ISpecialDayInstanceMessages {
  public string Name => name;

  public string[] Description => description;

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

  public IView GenerateStartMessage() {
    var result = new SimpleView {
      SpecialDayMessages.PREFIX, { "Today is a", Name, "day!" }
    };

    if (description.Length == 0) return result;

    result.Add(description[0]);

    for (var i = 1; i < description.Length; i++) {
      result.Add(SimpleView.NEWLINE);
      result.Add(SpecialDayMessages.PREFIX);
      result.Add(description[i]);
    }

    return result;
  }

  public virtual IView SpecialDayEnd() {
    var winner = PlayerUtil.GetAlive().FirstOrDefault()?.Team
      ?? CsTeam.Spectator;
    return new SimpleView {
      SpecialDayMessages.PREFIX,
      Name,
      "ended.",
      (winner == CsTeam.CounterTerrorist ? ChatColors.Blue : ChatColors.Red)
      + (winner == CsTeam.CounterTerrorist ? "Guards" : "Prisoners"),
      "won!"
    };
  }
}