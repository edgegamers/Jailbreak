using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.Weapon;

public interface IEquipBlocker {
  bool PreventEquip(CCSPlayerController player, CCSWeaponBase weapon);
}