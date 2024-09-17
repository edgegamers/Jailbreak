using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views;
using Jailbreak.Formatting.Views.RTD;
using Jailbreak.Public.Mod.RTD;

namespace Jailbreak.English.RTD;

public class RTDLocale : IRTDLocale, ILanguage<Formatting.Languages.English> {
  public static readonly string PREFIX =
    $" {ChatColors.Purple}[{ChatColors.LightPurple}RTD{ChatColors.Purple}]";

  public IView RewardSelected(IRTDReward reward) {
    var view = new SimpleView {
      PREFIX,
      ChatColors.Grey + "You rolled " + ChatColors.White + reward.Name
      + ChatColors.Grey + "."
    };

    if (reward.Description == null) return view;

    view.Add(SimpleView.NEWLINE);
    view.Add(PREFIX);
    view.Add(ChatColors.Grey + reward.Description);

    return view;
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
      PREFIX,
      ChatColors.Red + "You can only roll when round ends or while dead."
    };
  }

  public IView RollingDisabled() {
    return new SimpleView { PREFIX, ChatColors.Red + "Rolling is disabled." };
  }
}