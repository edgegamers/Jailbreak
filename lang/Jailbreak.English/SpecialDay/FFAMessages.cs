using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.SpecialDay;

public class FFAMessages : ISpecialDayMessages {
  public IView SpecialDayStart
    => new SimpleView {
      { ISpecialDayMessages.PREFIX, "Free For All has begun!" },
      SimpleView.NEWLINE, {
        ISpecialDayMessages.PREFIX,
        "Every man for themself, no camping, actively pursue!"
      }
    };

  IView ISpecialDayMessages.SpecialDayEnd(CsTeam winner) {
    var lastAlive = Utilities.GetPlayers()
     .FirstOrDefault(p => p is { PawnIsAlive: true });
    if (lastAlive == null)
      return new SimpleView {
        ISpecialDayMessages.PREFIX, "Free For All has ended! No one won!"
      };
    return new SimpleView {
      ISpecialDayMessages.PREFIX, "Free For All has ended!", lastAlive, "won!"
    };
  }
}