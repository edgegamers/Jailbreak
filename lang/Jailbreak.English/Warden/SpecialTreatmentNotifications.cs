using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.Warden;

public class SpecialTreatmentNotifications : ISpecialTreatmentNotifications,
  ILanguage<Formatting.Languages.English> {
  public IView Granted
    => new SimpleView {
      WardenNotifications.PREFIX,
      $"You now have {ChatColors.Green}special treatment{ChatColors.White}!"
    };

  public IView Revoked
    => new SimpleView {
      WardenNotifications.PREFIX,
      $"Your special treatment was {ChatColors.Red}removed{ChatColors.White}."
    };

  public IView GrantedTo(CCSPlayerController player) {
    return new SimpleView {
      WardenNotifications.PREFIX,
      player,
      $"now has {ChatColors.Green}Special Treatment{ChatColors.White}!"
    };
  }

  public IView RevokedFrom(CCSPlayerController player) {
    return new SimpleView {
      WardenNotifications.PREFIX,
      player,
      $"{ChatColors.Red}no longer {ChatColors.Grey}has Special Treatment."
    };
  }
}