using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Jailbreak.Formatting.Views;
using Jailbreak.Formatting.Views.Logging;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.RTD;
using Jailbreak.Public.Utils;

namespace Jailbreak.RTD;

public class RTDRewarder(IRichLogService logs, IRTDLocale locale)
  : IRTDRewarder, IPluginBehavior {
  private readonly Dictionary<int, IRTDReward> rewards = new();

  public bool HasReward(int id) { return GetReward(id) != null; }

  public IRTDReward? GetReward(int id) {
    return rewards.TryGetValue(id, out var reward) ? reward : null;
  }

  public bool SetReward(int id, IRTDReward reward) {
    if (!reward.PrepareReward(id)) return false;
    rewards[id] = reward;
    return true;
  }

  [GameEventHandler]
  public HookResult OnStart(EventRoundStart @event, GameEventInfo info) {
    foreach (var player in PlayerUtil.GetAlive()) {
      var id = player.UserId ?? -1;

      var reward = GetReward(id);
      if (reward == null) continue;

      logs.Append("Granted", reward.Name, "to", logs.Player(player));
      reward.GrantReward(id);
    }

    rewards.Clear();
    return HookResult.Continue;
  }
}