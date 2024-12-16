using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Jailbreak.Formatting.Views.Logging;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.RTD;
using Jailbreak.Public.Utils;

namespace Jailbreak.RTD;

public class RTDRewarder(IRichLogService logs) : IRTDRewarder, IPluginBehavior {
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
  public HookResult OnSpawn(EventPlayerSpawn @event, GameEventInfo info) {
    if (RoundUtil.IsWarmup()) return HookResult.Continue;
    var player = @event.Userid;

    if (player == null || !player.IsValid) return HookResult.Continue;

    var id = player.UserId ?? -1;

    var reward = GetReward(id);
    if (reward == null) return HookResult.Continue;
    if (!reward.CanGrantReward(player)) return HookResult.Continue;

    Server.RunOnTick(Server.TickCount + 2, () => {
      if (!player.IsValid) return;
      if (reward.Name != "Nothing")
        logs.Append("Granted", reward.Name, "to", logs.Player(player));
      reward.GrantReward(id);
      rewards.Remove(id);
    });
    return HookResult.Continue;
  }
}