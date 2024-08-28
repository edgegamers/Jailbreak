using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Mod.RTD;

namespace Jailbreak.English.RTD;

public class RTDLocale : IRTDLocale, ILanguage<Formatting.Languages.English> {
  public static readonly string PREFIX =
    $" {ChatColors.Purple}[{ChatColors.LightPurple}RTD{ChatColors.Purple}]";

  public IView RewardSelected(IRTDReward reward) {
    return new SimpleView {
      { PREFIX, "You rolled ", reward.Name + "." },
      SimpleView.NEWLINE,
      { PREFIX, reward.Description }
    };
  }

  public IView AlreadyRolled(IRTDReward reward) {
    return new SimpleView {
      PREFIX,
      ChatColors.Red + "You already rolled " + ChatColors.DarkRed + reward.Name
      + ChatColors.Red + "."
    };
  }

  public IView CannotRollYet() {
    return new SimpleView {
      PREFIX, "You can only roll once the round ends or you die."
    };
  }
}