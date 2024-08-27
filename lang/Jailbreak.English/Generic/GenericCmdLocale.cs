﻿using CounterStrikeSharp.API.Modules.Utils;
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
    new HiddenFormatObject(
      $" {ChatColors.DarkRed}[{ChatColors.LightRed}JB{ChatColors.DarkRed}]") {
      //	Hide in panorama and center text
      Plain = false, Panorama = false, Chat = true
    };

  public IView PlayerNotFound(string query) {
    return new SimpleView {
      PREFIX,
      $"{ChatColors.Red}Player '{ChatColors.LightBlue}{query}{ChatColors.Red}' not found."
    };
  }

  public IView PlayerFoundMultiple(string query) {
    return new SimpleView {
      PREFIX,
      $"{ChatColors.Red}Multiple players found for '{ChatColors.LightBlue}{query}{ChatColors.Red}'."
    };
  }

  public IView CommandOnCooldown(DateTime cooldownEndsAt) {
    var seconds = (int)(cooldownEndsAt - DateTime.Now).TotalSeconds;
    return new SimpleView {
      PREFIX,
      $"{ChatColors.Grey}Command is on cooldown for",
      seconds,
      $"{ChatColors.Grey}second" + (seconds == 1 ? "" : "s") + "."
    };
  }

  public IView InvalidParameter(string parameter, string expected) {
    return new SimpleView {
      PREFIX,
      $"{ChatColors.Red}Invalid parameter '{ChatColors.LightBlue}{parameter}{ChatColors.Red}',",
      "expected a" + (expected[0].IsVowel() ? "n" : ""),
      $"{ChatColors.White}{expected}{ChatColors.Red}."
    };
  }

  public IView NoPermissionMessage(string permission) {
    return new SimpleView {
      PREFIX,
      $"{ChatColors.DarkRed}This requires the {ChatColors.White}{permission}{ChatColors.Red} permission."
    };
  }

  public IView Error(string message) {
    return new SimpleView {
      PREFIX, $"{ChatColors.Red}An error occurred: {ChatColors.White}{message}"
    };
  }
}