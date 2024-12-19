using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Base;
using Jailbreak.Public.Mod.RTD;

namespace Jailbreak.Formatting.Views.RTD;

public interface IRTDLocale {
  public IView RewardSelected(IRTDReward reward);
  public IView AlreadyRolled(IRTDReward reward);
  public IView CannotRollYet();
  public IView RollingDisabled();
  public IView JackpotReward(CCSPlayerController winner, int credits);
}