using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views.Warden;

namespace Jailbreak.English.Warden;

public class WardenCmdRollLocale : IWardenCmdRollLocale,
  ILanguage<Formatting.Languages.English> {
  public IView Roll(int roll) {
    return new SimpleView {
      WardenLocale.PREFIX, "warden has rolled", roll, "!"
    };
  }
}