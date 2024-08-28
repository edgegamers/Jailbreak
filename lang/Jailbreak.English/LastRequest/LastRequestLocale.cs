using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Objects;
using Jailbreak.Formatting.Views.LastRequest;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.English.LastRequest;

public class LastRequestLocale : ILRLocale,
  ILanguage<Formatting.Languages.English> {
  public static readonly FormatObject PREFIX =
    new HiddenFormatObject(
      $" {ChatColors.Green}[{ChatColors.Lime}LR{ChatColors.Green}]") {
      //	Hide in panorama and center text
      Plain = false, Panorama = false, Chat = true
    };

  public IView LastRequestEnabled() {
    return new SimpleView {
      {
        PREFIX,
        $"Last Request activated. {ChatColors.Grey}Type {ChatColors.LightBlue}!lr{ChatColors.Grey} to start a last request."
      }
    };
  }

  public IView LastRequestDisabled() {
    return new SimpleView {
      {
        PREFIX,
        $"{ChatColors.Grey}Last Request {ChatColors.Red}disabled{ChatColors.Grey}."
      }
    };
  }

  public IView LastRequestNotEnabled() {
    return new SimpleView {
      { PREFIX, $"{ChatColors.Red}Last Request is not enabled." }
    };
  }

  public IView InvalidLastRequest(string query) {
    return new SimpleView { PREFIX, "Invalid Last Request: ", query };
  }

  public IView InformLastRequest(AbstractLastRequest lr) {
    return new SimpleView {
      PREFIX,
      lr.Prisoner,
      ChatColors.Grey + "is starting a",
      ChatColors.White + lr.Type.ToFriendlyString(),
      ChatColors.Grey + "LR against",
      lr.Guard
    };
  }

  public IView AnnounceLastRequest(AbstractLastRequest lr) {
    return new SimpleView {
      PREFIX,
      lr.Prisoner,
      "is doing a",
      lr.Type.ToFriendlyString(),
      "Last Request against",
      lr.Guard
    };
  }

  public IView LastRequestDecided(AbstractLastRequest lr, LRResult result) {
    var tNull = !lr.Prisoner.IsReal();
    var gNull = !lr.Guard.IsReal();
    if (tNull && gNull)
      return new SimpleView { PREFIX, "Last Request decided." };

    if (tNull && result == LRResult.PRISONER_WIN)
      return new SimpleView {
        PREFIX, lr.Guard, "lost the LR, but the prisoner left the game?"
      };

    if (gNull && result == LRResult.GUARD_WIN)
      return new SimpleView {
        PREFIX, lr.Prisoner, "lost the LR, but the guard left the game?"
      };

    return result switch {
      LRResult.TIMED_OUT => new SimpleView {
        PREFIX, ChatColors.Grey.ToString(), "Last Request timed out."
      },
      LRResult.INTERRUPTED => new SimpleView {
        PREFIX, ChatColors.Grey.ToString(), "Last Request interrupted."
      },
      _ => new SimpleView {
        PREFIX, result == LRResult.PRISONER_WIN ? lr.Prisoner : lr.Guard, "won."
      }
    };
  }

  public IView CannotLR(string reason) {
    return new SimpleView {
      PREFIX,
      $"{ChatColors.Red}You cannot LR: {ChatColors.White + reason + ChatColors.Red}."
    };
  }

  public IView CannotLR(CCSPlayerController player, string reason) {
    return new SimpleView {
      PREFIX,
      ChatColors.Red + "You cannot LR",
      player,
      ": " + ChatColors.White + reason + ChatColors.Red + "."
    };
  }

  public IView LastRequestCountdown(int seconds) {
    return new SimpleView { PREFIX, "Starting in", seconds, "..." };
  }

  public IView WinByDefault(CCSPlayerController player) {
    return new SimpleView { PREFIX, player, "won by default." };
  }

  public IView WinByHealth(CCSPlayerController player) {
    return new SimpleView { PREFIX, player, "won by health." };
  }

  public IView WinByReason(CCSPlayerController player, string reason) {
    return new SimpleView { PREFIX, player, "won by", reason + "." };
  }

  public IView Win(CCSPlayerController player) {
    return new SimpleView { PREFIX, player, "won." };
  }

  public IView DamageBlockedInsideLastRequest
    => new SimpleView { PREFIX, "You or they are in LR, damage blocked." };

  public IView DamageBlockedNotInSameLR
    => new SimpleView {
      PREFIX, "You are not in the same LR as them, damage blocked."
    };
}