using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views.Warden;

namespace Jailbreak.English.Warden;

public class WardenCmdOpenLocale : IWardenCmdOpenLocale,
  ILanguage<Formatting.Languages.English> {
  public IView CannotOpenYet(int seconds) {
    return new SimpleView {
      WardenLocale.PREFIX,
      "You must wait",
      seconds,
      "seconds before opening the cells."
    };
  }

  public IView AlreadyOpened
    => new SimpleView { WardenLocale.PREFIX, "Cells are already opened." };

  public IView CellsOpenedBy(CCSPlayerController? player) {
    return player == null ?
      new SimpleView {
        WardenLocale.PREFIX,
        $"{ChatColors.Blue}The warden {ChatColors.Default}opened the cells."
      } :
      new SimpleView { WardenLocale.PREFIX, player, "opened the cells." };
  }

  public IView CellsOpened
    => new SimpleView {
      WardenLocale.PREFIX, ChatColors.Grey + "Cells were auto-opened."
    };

  public IView CellsOpenedWithPrisoners(int prisoners) {
    return new SimpleView {
      WardenLocale.PREFIX,
      ChatColors.Grey + "Detected",
      prisoners,
      ChatColors.Grey + "prisoner" + (prisoners == 1 ? "" : "s")
      + " still in cells, opening..."
    };
  }

  public IView CellsOpenedSnitchPrisoners(int prisoners) {
    return new SimpleView {
      WardenLocale.PREFIX,
      ChatColors.Grey + "Detected",
      prisoners,
      ChatColors.Green + "prisoner" + (prisoners == 1 ? "" : "s")
      + " still in cells..."
    };
  }

  public IView OpeningFailed
    => new SimpleView { WardenLocale.PREFIX, "Failed to open the cells." };
}