using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views.Warden;

namespace Jailbreak.English.Warden;

public class WardenSTLocale : IWardenSTLocale,
  ILanguage<Formatting.Languages.English> {
  public IView Granted
    => new SimpleView {
      WardenLocale.PREFIX,
      $"You now have {ChatColors.Green}Special Treatment{ChatColors.White}!"
    };

  public IView Revoked
    => new SimpleView {
      WardenLocale.PREFIX,
      $"Your Special Treatment was {ChatColors.Red}removed{ChatColors.White}."
    };

  public IView GrantedTo(CCSPlayerController player) {
    return new SimpleView {
      WardenLocale.PREFIX,
      player,
      $"now has {ChatColors.Green}Special Treatment{ChatColors.White}!"
    };
  }

  public IView RevokedFrom(CCSPlayerController player) {
    return new SimpleView {
      WardenLocale.PREFIX,
      player,
      $"{ChatColors.Red}no longer {ChatColors.Grey}has Special Treatment."
    };
  }
}