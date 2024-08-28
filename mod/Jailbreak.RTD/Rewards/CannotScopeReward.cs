using System.Collections.Immutable;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.RTD;

namespace Jailbreak.RTD.Rewards;

public class CannotScopeReward : IRTDReward {
  private readonly BasePlugin plugin;

  public CannotScopeReward(BasePlugin plugin, WeaponType blocked) : this(plugin,
    blocked.GetItems().ToArray()) { }

  public CannotScopeReward(BasePlugin plugin, params string[] weapons) {
    this.plugin = plugin;
    this.plugin.RegisterEventHandler<EventRoundEnd>(onRoundEnd);
  }

  public string Name => "Cannot Scope";

  public string Description => $"You will not be able to scope next round.";

  private bool registered;

  private readonly HashSet<int> blockedPlayerIDs = [];

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
      disableScope(player);
    }
  }

  private void disableScope(CCSPlayerController player) {
    if (!player.IsReal()) return;
    var pawn = player.PlayerPawn.Value;
    if (pawn == null || !pawn.IsValid) return;
    var weaponServices = pawn.WeaponServices;
    if (weaponServices == null) return;
    var activeWeapon = weaponServices.ActiveWeapon.Value;
    if (activeWeapon == null || !activeWeapon.IsValid) return;
    activeWeapon.NextSecondaryAttackTick = Server.TickCount + 500;
  }
}