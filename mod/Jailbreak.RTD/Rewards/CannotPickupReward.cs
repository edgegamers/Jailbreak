using System.Collections.Immutable;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.RTD;

namespace Jailbreak.RTD.Rewards;

public class CannotPickupReward : IRTDReward {
  private readonly BasePlugin plugin;
  private readonly ImmutableHashSet<string> blockedWeapons;

  public CannotPickupReward(BasePlugin plugin, WeaponType blocked) : this(
    plugin, blocked.GetItems().ToArray()) {
    Name = $"Cannot Pickup {blocked}";
  }

  public CannotPickupReward(BasePlugin plugin, params string[] weapons) {
    this.plugin = plugin;
    this.plugin.RegisterEventHandler<EventItemPickup>(onPickup);
    this.plugin.RegisterEventHandler<EventRoundEnd>(onRoundEnd);

    blockedWeapons = weapons.ToImmutableHashSet();
    Name =
      $"Cannot Pickup {string.Join(", ", blockedWeapons.Select(s => s.GetFriendlyWeaponName()))}";
  }

  public string Name { get; }

  public string Description
    => $"You will not be able to pickup {Name} next round.";

  private readonly HashSet<int> blockedPlayerIDs = [];

  public bool GrantReward(CCSPlayerController player) {
    if (player.UserId == null) return false;
    blockedPlayerIDs.Add(player.UserId.Value);
    return true;
  }

  private HookResult onRoundEnd(EventRoundEnd @event, GameEventInfo info) {
    blockedPlayerIDs.Clear();
    return HookResult.Continue;
  }

  private HookResult onPickup(EventItemPickup @event, GameEventInfo info) {
    var player = @event.Userid;
    if (player == null || !player.IsValid || player.UserId == null)
      return HookResult.Continue;
    if (!blockedPlayerIDs.Contains(player.UserId.Value))
      return HookResult.Continue;
    var weapon = "weapon_" + @event.Item;
    if (!blockedWeapons.Contains(weapon)) return HookResult.Continue;
    player.RemoveItemByDesignerName(weapon, true);
    return HookResult.Continue;
  }
}