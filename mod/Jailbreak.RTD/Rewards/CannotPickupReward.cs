using System.Collections.Immutable;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.RTD;

namespace Jailbreak.RTD.Rewards;

public class CannotPickupReward : IRTDReward {
  private readonly HashSet<int> blockedPlayerIDs = [];
  private readonly ImmutableHashSet<string> blockedWeapons;

  public CannotPickupReward(BasePlugin plugin, WeaponType blocked) : this(
    plugin, blocked.GetItems().ToArray()) {
    NameShort = blocked.ToString();
  }

  public CannotPickupReward(BasePlugin plugin, params string[] weapons) {
    plugin.RegisterEventHandler<EventItemPickup>(onPickup);
    plugin.RegisterEventHandler<EventRoundEnd>(onRoundEnd);

    blockedWeapons = weapons.ToImmutableHashSet();
    NameShort = string.Join(", ",
      blockedWeapons.Select(s => s.GetFriendlyWeaponName()));
  }

  public virtual string Name => $"Cannot Pickup {NameShort}";
  public string NameShort { get; }

  public virtual string Description
    => $"You will not be able to pickup {NameShort} next round.";

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