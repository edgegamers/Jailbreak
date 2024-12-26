using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.Weapon;

public interface IUseBlocker {
  public bool PreventUse(CCSPlayerController player, CBasePlayerWeapon weapon);
}