using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Objects;
using Jailbreak.Formatting.Views.SpecialDay;

namespace Jailbreak.English.SpecialDay;

public class SDLocale : ISDLocale, ILanguage<Formatting.Languages.English> {
  public static readonly FormatObject PREFIX =
    new HiddenFormatObject(
      $" {ChatColors.BlueGrey}[{ChatColors.Green}S{ChatColors.Blue}D{ChatColors.BlueGrey}]") {
      //	Hide in panorama and center text
      Plain = false, Panorama = false, Chat = true
    };

  public IView SpecialDayRunning(string name) {
    return new SimpleView {
      PREFIX,
      ChatColors.DarkRed + name,
      ChatColors.Red + "is currently running."
    };
  }

  public IView InvalidSpecialDay(string name) {
    return new SimpleView {
      PREFIX,
      ChatColors.DarkRed + name,
      ChatColors.Red + "is not a valid special day."
    };
  }

  public IView SpecialDayCooldown(int rounds) {
    return new SimpleView {
      PREFIX,
      "You must wait",
      rounds,
      "more round" + (rounds == 1 ? "" : "s")
      + " before starting a special day."
    };
  }

  public IView TooLateForSpecialDay(int maxTime) {
    return new SimpleView {
      PREFIX,
      "You must start a special day within",
      maxTime,
      "second" + (maxTime == 1 ? "" : "s") + " of the round start."
    };
  }
}