using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.Warden;

public class OpenCommandNotifications : IOpenCommandMessages,
  ILanguage<Formatting.Languages.English> {
  public IView CannotOpenYet(int seconds) {
    return new SimpleView {
      WardenNotifications.PREFIX,
      "You must wait",
      seconds,
      "seconds before opening the cells."
    };
  }

  public IView CellsOpened
    => new SimpleView {
      WardenNotifications.PREFIX, "The warden opened cells."
    };

  public IView OpeningFailed
    => new SimpleView {
      WardenNotifications.PREFIX, "Failed to open the cells."
    };
}