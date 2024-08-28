using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.RTD;
using Jailbreak.Public.Utils;

namespace Jailbreak.RTD;

public class RTDCommand(IRTDRewarder rewarder, IRewardGenerator generator,
  IRTDLocale locale, IGenericCmdLocale generic) : IPluginBehavior {
  private bool inBetweenRounds;

  [ConsoleCommand("css_rtd", "Roll the dice!")]
  public void Command_RTD(CCSPlayerController? executor, CommandInfo info) {
    if (executor == null) return;
    var bypass = AdminManager.PlayerHasPermissions(executor, "@css/root")
      && info.ArgCount == 2;

    var old = rewarder.GetReward(executor);
    if (!bypass && old != null) {
      locale.AlreadyRolled(old).ToChat(executor);
      return;
    }

    if (!bypass && !inBetweenRounds && !RoundUtil.IsWarmup()
      && executor.PawnIsAlive) {
      locale.CannotRollYet().ToChat(executor);
      return;
    }

    var reward = generator.GenerateReward(executor);
    if (bypass) {
      if (!int.TryParse(info.GetArg(1), out var slot)) {
        generic.InvalidParameter(info.GetArg(1), "integer").ToChat(executor);
        return;
      }

      if (slot != -1) {
        var rewards = generator.ToList();

        if (slot < 0 || slot >= rewards.Count) {
          generic.InvalidParameter(info.GetArg(1), "0-" + (rewards.Count - 1))
           .ToChat(executor);
          return;
        }

        reward = generator.ToList()[slot].Item1;
      }
    }

    rewarder.SetReward(executor, reward);
    locale.RewardSelected(reward).ToChat(executor);
  }

  [GameEventHandler]
  public HookResult OnEnd(EventRoundEnd @event, GameEventInfo info) {
    inBetweenRounds = true;
    return HookResult.Continue;
  }

  [GameEventHandler]
  public HookResult OnStart(EventRoundStart @event, GameEventInfo info) {
    inBetweenRounds = false;
    return HookResult.Continue;
  }
}