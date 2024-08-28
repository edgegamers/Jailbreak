using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Objects;
using Jailbreak.Formatting.Views.Warden;
using Microsoft.Extensions.Primitives;

namespace Jailbreak.English.Mute;

public class WardenPeaceLocale : IWardenPeaceLocale,
  ILanguage<Formatting.Languages.English> {
  private static readonly FormatObject PREFIX =
    new HiddenFormatObject(
      $" {ChatColors.DarkBlue}[{ChatColors.LightBlue}Voice{ChatColors.DarkBlue}]{ChatColors.Grey} ") {
      Plain = false, Panorama = false, Chat = true
    };

  public IView PeaceEnactedByAdmin(int seconds) {
    return new SimpleView {
      PREFIX,
      $"{ChatColors.Red}An admin {ChatColors.White}enacted peace for",
      seconds,
      "second" + (seconds == 1 ? "" : "s") + "."
    };
  }

  public IView WardenEnactedPeace(int seconds) {
    return new SimpleView {
      PREFIX,
      $"{ChatColors.Blue}The warden {ChatColors.White}enacted peace for",
      seconds,
      "seconds."
    };
  }

  public IView GeneralPeaceEnacted(int seconds) {
    return new SimpleView {
      PREFIX,
      "Peace was enacted for",
      seconds,
      "second" + (seconds == 1 ? "" : "s") + "."
    };
  }

  public IView UnmutedGuards
    => new SimpleView {
      { PREFIX, $"{ChatColors.Blue}Guards {ChatColors.Grey}were unmuted." }
    };

  public IView UnmutedPrisoners
    => new SimpleView {
      {
        PREFIX, $"{ChatColors.LightRed}Prisoners {ChatColors.Grey}were unmuted."
      }
    };

  public IView MuteReminder
    => new SimpleView {
      { PREFIX, ChatColors.Red + "You are currently muted!" }
    };

  public IView PeaceReminder
    => new SimpleView {
      {
        PREFIX,
        $"Peace is currently active. {ChatColors.Red}You should only be talking if absolutely necessary!"
      }
    };

  public IView DeadReminder
    => new SimpleView {
      { PREFIX, $"{ChatColors.Red}You are dead and cannot speak." }
    };

  public IView AdminDeadReminder
    => new SimpleView {
      {
        PREFIX, "You are dead.",
        $"{ChatColors.Red}You should only be talking if absolutely necessary!"
      }
    };

  public IView PeaceActive
    => new SimpleView { { PREFIX, "Peace is currently active." } };
}