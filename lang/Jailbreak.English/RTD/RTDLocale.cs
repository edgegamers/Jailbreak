using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views.RTD;
using Jailbreak.Public.Mod.RTD;

namespace Jailbreak.English.RTD;

public class RTDLocale : IRTDLocale, ILanguage<Formatting.Languages.English> {
  public static readonly string PREFIX = $" {ChatColors.LightBlue}RTD>";

  public IView RewardSelected(IRTDReward reward) {
    var view = new SimpleView {
      PREFIX,
      "You rolled " + ChatColors.BlueGrey + reward.Name + ChatColors.Grey + "."
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
      "You already rolled " + ChatColors.Red + reward.Name + ChatColors.Grey
      + "."
    };
  }

  public IView CannotRollYet() {
    return new SimpleView {
      PREFIX, "You can only roll when round ends or while dead."
    };
  }

  public IView RollingDisabled() {
    return new SimpleView { PREFIX, "Rolling is disabled." };
  }

  public IView JackpotReward(CCSPlayerController winner, int credits) {
    return new SimpleView {
      PREFIX,
      winner,
      "won the jackpot of ",
      credits,
      " credits!"
    };
  }
}