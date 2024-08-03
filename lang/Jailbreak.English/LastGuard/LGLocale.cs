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
    new HiddenFormatObject(
      $" {ChatColors.DarkRed}[{ChatColors.LightRed}Last Guard{ChatColors.DarkRed}]") {
      //	Hide in panorama and center text
      Plain = false, Panorama = false, Chat = true
    };

  public IView
    LGStarted(CCSPlayerController lastGuard, int ctHealth, int tHealth) {
    return new SimpleView {
      SimpleView.NEWLINE, {
        PREFIX,
        $"{ChatColors.Red}All Ts are rebels! {ChatColors.DarkRed}LAST GUARD{ChatColors.Red} must kill until two prisoners alive (LR)."
      },
      SimpleView.NEWLINE, {
        PREFIX, lastGuard, ChatColors.Red + "has", ctHealth,
        $"{ChatColors.Red}health and Ts have", tHealth,
        $"{ChatColors.Red}health."
      }
    };
  }
}