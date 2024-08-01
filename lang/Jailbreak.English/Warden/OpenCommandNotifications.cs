using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Utils;

namespace Jailbreak.English.Warden;

public class OpenCommandNotifications : IOpenCommandMessages {
  public IView OpenResult(Sensitivity sensitivity) {
    throw new NotImplementedException();
  }

  public IView OpenedCells => new SimpleView { "The warden opened the cells." };
}