using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.SpecialDay;

public class FfaInstanceMessages : ISpecialDayInstanceMessages,
  ILanguage<Formatting.Languages.English> {
  public string Name => "Free for All";

  public IView SpecialDayStart
    => new SimpleView {
      { ISpecialDayMessages.PREFIX, Name, "has begun!" },
      SimpleView.NEWLINE, {
        ISpecialDayMessages.PREFIX,
        "Everyone for themselves, no camping, actively pursue!"
      }
    };

  IView ISpecialDayInstanceMessages.SpecialDayEnd(CsTeam winner) {
    var lastAlive = Utilities.GetPlayers()
     .FirstOrDefault(p => p is { PawnIsAlive: true });
    if (lastAlive == null)
      return new SimpleView {
        ISpecialDayMessages.PREFIX, Name, "has ended! No one won!"
      };
    return new SimpleView {
      ISpecialDayMessages.PREFIX,
      Name,
      "has ended!",
      lastAlive,
      "won!"
    };
  }

  public IView DamageEnablingIn(int seconds)
    => new SimpleView {
      ISpecialDayMessages.PREFIX,
      "Damage will be enabled in",
      seconds,
      "seconds."
    };

  IView Begin => new SimpleView { ISpecialDayMessages.PREFIX, "GO!" };
}