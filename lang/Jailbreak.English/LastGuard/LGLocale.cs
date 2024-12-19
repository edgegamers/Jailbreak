using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Objects;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.LastGuard;

public class LGLocale : ILGLocale, ILanguage<Formatting.Languages.English> {
  private static readonly FormatObject PREFIX =
    new HiddenFormatObject($" {ChatColors.DarkBlue}Guard>") {
      //	Hide in panorama and center text
      Plain = false, Panorama = false, Chat = true
    };

  public IView
    LGStarted(CCSPlayerController lastGuard, int ctHealth, int tHealth) {
    return new SimpleView {
      SimpleView.NEWLINE, {
        PREFIX,
        $"{ChatColors.Grey}All Ts are rebels! {ChatColors.DarkRed}LAST GUARD{ChatColors.Grey} must kill until two prisoners alive ({ChatColors.BlueGrey}LR{ChatColors.Grey})."
      },
      SimpleView.NEWLINE, {
        lastGuard, ChatColors.Grey + "has", ctHealth,
        $"{ChatColors.Grey}health, Ts have", tHealth,
        $"{ChatColors.Grey}health total."
      }
    };
  }
}