using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Objects;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.Warden;

public class SpecialTreatmentNotifications : ISpecialTreatmentNotifications,
  ILanguage<Formatting.Languages.English> {
  public static readonly FormatObject PREFIX =
    new HiddenFormatObject(
      $" {ChatColors.Lime}[{ChatColors.Green}ST{ChatColors.Lime}]") {
      //	Hide in panorama and center text
      Plain = false, Panorama = false, Chat = true
    };

  public IView Granted
    => new SimpleView {
      PREFIX,
      $"You now have {ChatColors.Green}special treatment{ChatColors.White}!"
    };

  public IView Revoked
    => new SimpleView {
      PREFIX,
      $"Your special treatment was {ChatColors.Red}removed{ChatColors.White}."
    };

  public IView GrantedTo(CCSPlayerController player) {
    return new SimpleView {
      PREFIX,
      player,
      $"now has {ChatColors.Green}Special Treatment{ChatColors.White}!"
    };
  }

  public IView RevokedFrom(CCSPlayerController player) {
    return new SimpleView {
      PREFIX,
      player,
      $"{ChatColors.Red}no longer {ChatColors.Grey}has Special Treatment."
    };
  }
}