using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Views.Logging;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;

namespace Jailbreak.Logs.Listeners;

public class LogEntityParentListeners(IRichLogService logs) : IPluginBehavior {
  private static readonly string[] WEAPON_STRINGS = [
    "weapon_ak47", "weapon_aug", "weapon_awp", "weapon_bizon", "weapon_cz75a",
    "weapon_deagle", "weapon_famas", "weapon_fiveseven", "weapon_g3sg1",
    "weapon_galilar", "weapon_glock", "weapon_hkp2000", "weapon_m249",
    "weapon_m4a1", "weapon_m4a1_silencer", "weapon_m4a4", "weapon_mac10",
    "weapon_mag7", "weapon_mp5sd", "weapon_mp7", "weapon_mp9", "weapon_negev",
    "weapon_nova", "weapon_p250", "weapon_p90", "weapon_revolver",
    "weapon_sawedoff", "weapon_scar20", "weapon_sg553", "weapon_sg556",
    "weapon_ssg08", "weapon_taser", "weapon_tec9", "weapon_ump45",
    "weapon_usp_silencer", "weapon_xm1014"
  ];

  public void Start(BasePlugin _parent) {
    _parent
     .RegisterListener<
        CounterStrikeSharp.API.Core.Listeners.OnEntityParentChanged>(
        OnEntityParentChanged);
  }

  public void OnEntityParentChanged(CEntityInstance affectedEntity,
    CEntityInstance newParent) {
    if (!affectedEntity.IsValid
      || !WEAPON_STRINGS.Contains(affectedEntity.DesignerName))
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
}