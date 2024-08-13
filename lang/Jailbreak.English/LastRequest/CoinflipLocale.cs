using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Views.LastRequest;

namespace Jailbreak.English.LastRequest;

public class CoinflipLocale : LastRequestLocale, ILRCFLocale {
  public IView FailedToChooseInTime(bool choice) {
    return new SimpleView {
      PREFIX,
      "You failed to choose in time, defaulting to" + ChatColors.Green,
      choice ? "Heads" : "Tails"
    };
  }

  public IView GuardChose(CCSPlayerController guard, bool choice) {
    return new SimpleView {
      PREFIX,
      guard,
      "chose" + ChatColors.Green,
      choice ? "Heads" : "Tails",
      ChatColors.Default + ", flipping..."
    };
  }

  public IView CoinLandsOn(bool heads) {
    return new SimpleView {
      PREFIX, "The coin lands on" + ChatColors.Green, heads ? "Heads" : "Tails"
    };
  }
}