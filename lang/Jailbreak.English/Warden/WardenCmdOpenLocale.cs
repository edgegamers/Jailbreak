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
    => new SimpleView { WardenLocale.PREFIX, "You already opened cells." };

  public IView CellsOpened
    => new SimpleView { WardenLocale.PREFIX, "The warden opened cells." };

  public IView OpeningFailed
    => new SimpleView { WardenLocale.PREFIX, "Failed to open the cells." };
}