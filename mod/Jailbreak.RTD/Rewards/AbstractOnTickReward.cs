using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.RTD;

namespace Jailbreak.RTD.Rewards;

public abstract class AbstractOnTickReward : IRTDReward {
  private readonly HashSet<int> blockedPlayerIDs = [];
  private readonly BasePlugin plugin;

  private bool registered;

  public AbstractOnTickReward(BasePlugin plugin) {
    this.plugin = plugin;
    this.plugin.RegisterEventHandler<EventRoundEnd>(onRoundEnd);
  }

  public abstract string Name { get; }
  public abstract string? Description { get; }

  public bool GrantReward(CCSPlayerController player) {
    if (player.UserId == null) return false;
    if (!registered) {
      plugin.RegisterListener<Listeners.OnTick>(onTick);
      registered = true;
    }

    blockedPlayerIDs.Add(player.UserId.Value);
    return true;
  }

  private HookResult onRoundEnd(EventRoundEnd @event, GameEventInfo info) {
    blockedPlayerIDs.Clear();
    plugin.RemoveListener<Listeners.OnTick>(onTick);
    registered = false;
    return HookResult.Continue;
  }

  private void onTick() {
    registered = true;
    if (blockedPlayerIDs.Count == 0) {
      plugin.RemoveListener<Listeners.OnTick>(onTick);
      registered = false;
      return;
    }

    foreach (var player in blockedPlayerIDs.Select(
      Utilities.GetPlayerFromUserid)) {
      if (player == null || player.UserId == null || !player.IsValid) continue;
      tick(player);
    }
  }

  abstract protected void tick(CCSPlayerController player);
}