using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Objects;

namespace Jailbreak.Formatting.Views;

public interface ISpecialDayMessages {
  public static readonly FormatObject PREFIX =
    new HiddenFormatObject(
      $" {ChatColors.BlueGrey}[{ChatColors.Green}S{ChatColors.Blue}D{ChatColors.BlueGrey}]") {
      //	Hide in panorama and center text
      Plain = false, Panorama = false, Chat = true
    };

  public IView SpecialDayRunning(string name);
  public IView InvalidSpecialDay(string name);

  public IView SpecialDayCooldown(int rounds);
}