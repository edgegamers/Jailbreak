using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.SpecialDay;

public class FfaInstanceMessages() : SoloDayMessages("Free for All",
  "Everyone for themselves, no camping, actively pursue!") {
  public IView Begin => new SimpleView { ISpecialDayMessages.PREFIX, "GO!" };
}