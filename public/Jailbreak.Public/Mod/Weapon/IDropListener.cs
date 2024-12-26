using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.Weapon;

public interface IDropListener {
  void OnWeaponDrop(CCSPlayerController player, CCSWeaponBase weapon);
}