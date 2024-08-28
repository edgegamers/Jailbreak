using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views.Warden;
using Jailbreak.Public.Mod.Warden;

namespace Jailbreak.English.Warden;

public class WardenCmdSoccerLocale : IWardenCmdSoccerLocale,
  ILanguage<Formatting.Languages.English> {
  public IView SoccerSpawned
    => new SimpleView {
      WardenLocale.PREFIX,
      ChatColors.Blue + "The warden" + ChatColors.Grey
      + " spawned a soccer ball."
    };

  public IView SpawnFailed
    => new SimpleView {
      WardenLocale.PREFIX, ChatColors.Red + "Failed to spawn a soccer ball."
    };

  public IView TooManySoccers
    => new SimpleView { WardenLocale.PREFIX, "Too many soccer balls." };
}