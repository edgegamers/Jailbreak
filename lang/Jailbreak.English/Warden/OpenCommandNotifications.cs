using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Utils;

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

  public IView OpenResult(Sensitivity? sensitivity) {
    return sensitivity switch {
      Sensitivity.NAME_CELL_DOOR or Sensitivity.NAME_CELL => new SimpleView {
        WardenNotifications.PREFIX, "The warden opened the cell doors."
      },
      Sensitivity.TARGET_CELL_DOOR or Sensitivity.TARGET_CELL => new
        SimpleView {
          WardenNotifications.PREFIX,
          "The warden attempted to open the cell door."
        },
      Sensitivity.ANY_WITH_TARGET => new SimpleView {
        WardenNotifications.PREFIX, "Attempting to open cell coors..."
      },
      _ => new SimpleView {
        WardenNotifications.PREFIX, "Could not open cell doors."
      }
    };
  }
}