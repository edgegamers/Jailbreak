using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Jailbreak.Formatting.Views.Logging;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;

namespace Jailbreak.Logs.Listeners;

public class LogEntityParentListeners(IRichLogService logs) : IPluginBehavior {
  private readonly HashSet<int> recentWeaponEvents = new();

  public void Start(BasePlugin _parent) {
    _parent
     .RegisterListener<
        CounterStrikeSharp.API.Core.Listeners.OnEntityParentChanged>(
        OnEntityParentChanged);
  }

  public void OnEntityParentChanged(CEntityInstance affectedEntity,
    CEntityInstance newParent) {
    if (!affectedEntity.IsValid) return;
    if (!Tag.WEAPONS.Contains(affectedEntity.DesignerName)
      && !Tag.UTILITY.Contains(affectedEntity.DesignerName))
      return;

    var weaponEntity =
      Utilities.GetEntityFromIndex<CCSWeaponBase>((int)affectedEntity.Index);
    if (weaponEntity == null
      || weaponEntity.PrevOwner.Get()?.OriginalController.Get() == null)
      return;

    var weaponOwner = weaponEntity.PrevOwner.Get()?.OriginalController.Get();
    if (weaponOwner == null) return;

    if (!newParent.IsValid) // a.k.a parent is world
    {
      logs.Append(logs.Player(weaponOwner),
        $"dropped their {weaponEntity.ToFriendlyString()}");
      return;
    }

    if (!recentWeaponEvents.Add((int)weaponEntity.Index)) {
      recentWeaponEvents.Remove((int)weaponEntity.Index);
      return;
    }

    var weaponPickerUpper = Utilities
     .GetEntityFromIndex<CCSPlayerPawn>((int)newParent.Index)
    ?.OriginalController.Get();
    if (weaponPickerUpper == null) return;

    if (weaponPickerUpper == weaponOwner) {
      logs.Append(weaponPickerUpper,
        $"picked up their {weaponEntity.ToFriendlyString()}");
      return;
    }

    logs.Append(weaponPickerUpper, "picked up", logs.Player(weaponOwner),
      $"{weaponEntity.ToFriendlyString()}");
  }

  [GameEventHandler]
  public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info) {
    recentWeaponEvents.Clear();
    return HookResult.Continue;
  }
}