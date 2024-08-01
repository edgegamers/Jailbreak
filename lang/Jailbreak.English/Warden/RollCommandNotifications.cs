using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.Warden;

public class RollCommandNotifications : IRollCommandNotications,
  ILanguage<Formatting.Languages.English> {
  public IView Roll(int roll) {
    return new SimpleView {
      WardenNotifications.PREFIX, "warden has rolled", roll, "!"
    };
  }
}