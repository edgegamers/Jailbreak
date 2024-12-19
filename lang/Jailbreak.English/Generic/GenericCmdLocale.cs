using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Objects;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Extensions;

namespace Jailbreak.English.Generic;

public class GenericCmdLocale : IGenericCmdLocale,
  ILanguage<Formatting.Languages.English> {
  private static readonly FormatObject PREFIX =
    new HiddenFormatObject($" {ChatColors.LightBlue}Server>") {
      //	Hide in panorama and center text
      Plain = false, Panorama = false, Chat = true
    };

  public IView PlayerNotFound(string query) {
    return new SimpleView {
      PREFIX,
      $"Player '{ChatColors.BlueGrey}{query}{ChatColors.Grey}' not found."
    };
  }

  public IView PlayerFoundMultiple(string query) {
    return new SimpleView {
      PREFIX,
      $"Multiple players found for '{ChatColors.BlueGrey}{query}{ChatColors.Grey}'."
    };
  }

  public IView CommandOnCooldown(DateTime cooldownEndsAt) {
    var seconds = (int)(cooldownEndsAt - DateTime.Now).TotalSeconds;
    return new SimpleView {
      PREFIX,
      "Command is on cooldown for",
      seconds,
      "second" + (seconds == 1 ? "" : "s") + "."
    };
  }

  public IView InvalidParameter(string parameter, string expected) {
    return new SimpleView {
      PREFIX,
      $"Invalid parameter '{ChatColors.BlueGrey}{parameter}{ChatColors.Grey}',",
      "expected a" + (expected[0].IsVowel() ? "n" : ""),
      $"{ChatColors.BlueGrey}{expected}{ChatColors.Grey}."
    };
  }

  public IView NoPermissionMessage(string permission) {
    return new SimpleView {
      PREFIX,
      $"This requires the {ChatColors.BlueGrey}{permission}{ChatColors.Grey} permission."
    };
  }

  public IView Error(string message) {
    return new SimpleView {
      PREFIX, $"An error occurred: {ChatColors.Red}{message}{ChatColors.Grey}."
    };
  }
}