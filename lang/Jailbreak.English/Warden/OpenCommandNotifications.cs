using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Utils;

namespace Jailbreak.English.Warden;

public class OpenCommandNotifications : IOpenCommandMessages,
  ILanguage<Formatting.Languages.English> {
  public IView OpenResult(Sensitivity sensitivity) {
    return new SimpleView { "foobar" };
  }

  public IView OpenedCells => new SimpleView { "The warden opened the cells." };
}