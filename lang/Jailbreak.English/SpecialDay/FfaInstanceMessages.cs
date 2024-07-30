using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.SpecialDay;

public class FfaInstanceMessages(string name, string desc)
  : SoloDayMessages(name, desc) {
  public IView Begin => new SimpleView { ISpecialDayMessages.PREFIX, "GO!" };
}