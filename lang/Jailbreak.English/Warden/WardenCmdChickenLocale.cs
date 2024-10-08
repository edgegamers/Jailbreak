﻿using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views.Warden;

namespace Jailbreak.English.Warden;

public class WardenCmdChickenLocale : IWardenCmdChickenLocale,
  ILanguage<Formatting.Languages.English> {
  public IView ChickenSpawned
    => new SimpleView {
      WardenLocale.PREFIX,
      ChatColors.Blue + "The warden" + ChatColors.Grey + " spawned a chicken."
    };

  public IView SpawnFailed
    => new SimpleView {
      WardenLocale.PREFIX, ChatColors.Red + "Failed to spawn a chicken."
    };

  public IView TooManyChickens
    => new SimpleView { WardenLocale.PREFIX, "Too many chickens." };
}