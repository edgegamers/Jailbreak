using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Objects;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.Rebel;

public class RebelLocale : IRebelLocale,
  ILanguage<Formatting.Languages.English> {
  public static readonly FormatObject PREFIX =
    new HiddenFormatObject(
      $" {ChatColors.DarkRed}[{ChatColors.LightRed}Rebel{ChatColors.DarkRed}]") {
      //	Hide in panorama and center text
      Plain = false, Panorama = false, Chat = true
    };

  public IView NoLongerRebel
    => new SimpleView { PREFIX, "You are no longer red." };
}