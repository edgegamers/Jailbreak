using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Extensions;

public static class WeaponExtensions {
  public static string ToFriendlyString(this CCSWeaponBase weaponEntity) {
    return weaponEntity.DesignerName.GetFriendlyWeaponName();
  }

  public static string GetFriendlyWeaponName(this string designerName) {
    switch (designerName) {
      case "weapon_ak47":
        return "AK47";
      case "weapon_aug":
        return "AUG";
      case "weapon_awp":
        return "AWP";
      case "weapon_bizon":
        return "Bizon";
      case "weapon_cz75a":
        return "CZ75";
      case "weapon_deagle":
        return "Desert Eagle";
      case "weapon_elite":
        return "Dualies";
      case "weapon_famas":
        return "Famas";
      case "weapon_fiveseven":
        return "Five Seven";
      case "weapon_g3sg1":
        return "G3SG1";
      case "weapon_galilar":
        return "Galil";
      case "weapon_glock":
        return "Glock 18";
      case "weapon_hkp2000":
        return "HPK2000";
      case "weapon_m249":
        return "M249";
      case "weapon_m4a1":
        return "M4A1";
      case "weapon_m4a1_silencer":
        return "M4A1S";
      case "weapon_m4a4":
        return "M4A4";
      case "weapon_mac10":
        return "MAC10";
      case "weapon_mag7":
        return "MAG7";
      case "weapon_mp5sd":
        return "MP5SD";
      case "weapon_mp7":
        return "MP7";
      case "weapon_mp9":
        return "MP9";
      case "weapon_negev":
        return "Negev";
      case "weapon_nova":
        return "Nova";
      case "weapon_p250":
        return "P250";
      case "weapon_p90":
        return "P90";
      case "weapon_revolver":
        return "Revolver";
      case "weapon_sawedoff":
        return "Sawed Off";
      case "weapon_scar20":
        return "Scar20";
      case "weapon_sg553":
        return "SG553";
      case "weapon_sg556":
        return "SG556";
      case "weapon_ssg08":
        return "SSG08";
      case "weapon_taser":
        return "Zeus";
      case "weapon_tec9":
        return "Tec9";
      case "weapon_ump45":
        return "UMP45";
      case "weapon_usp_silencer":
        return "USPS";
      case "weapon_xm1014":
        return "XM1014";
      case "item_kevlar":
        return "Kevlar";
      case "item_assaultsuit":
        return "Kevlar Helmet";
      case "weapon_snowball":
        return "Snowball";
      case "weapon_shield":
        return "Shield";
      case "weapon_c4":
        return "Bomb";
      case "weapon_healthshot":
        return "Healthshot";
      case "weapon_breachcharge":
        return "Breach Charge";
      case "weapon_tablet":
        return "Tablet";
      case "weapon_bumpmine":
        return "Bumpmine";
      case "weapon_smokegrenade":
        return "Smoke Grenade";
      case "weapon_flashbang":
        return "Flashbang";
      case "weapon_hegrenade":
        return "HE Grenade";
      case "weapon_molotov":
        return "Molotov";
      case "weapon_incgrenade":
        return "Incendiary Grenade";
      case "weapon_decoy":
        return "Decoy Grenade";
      case "weapon_tagrenade":
        return "TAGrenade";
      case "weapon_frag":
        return "Frag Grenade";
      case "weapon_firebomb":
        return "Firebomb";
      case "weapon_diversion":
        return "Diversion";
      case "weapon_knife_t":
        return "Knife";
      case "weapon_knife":
        return "Knife";
      default:
        return "UNKNOWN: Please Contact Tech";
    }
  }

  public static void AddBulletsToMagazine(this CBasePlayerWeapon? weapon,
    int bullets) {
    if (weapon == null) return;
    if (weapon.Clip1 + bullets > weapon.VData!.MaxClip1) {
      var overflowBullets = weapon.Clip1 + bullets - weapon.VData!.MaxClip1;
      weapon.Clip1          =  weapon.VData!.MaxClip1;
      weapon.ReserveAmmo[0] += overflowBullets;
    } else { weapon.Clip1 += bullets; }

    Utilities.SetStateChanged(weapon, "CBasePlayerWeapon", "m_iClip1");
    Utilities.SetStateChanged(weapon, "CBasePlayerWeapon", "m_pReserveAmmo");
  }

  public static void SetAmmo(this CBasePlayerWeapon? weapon, int clip,
    int reserve) {
    if (weapon == null) return;
    weapon.Clip1          = clip;
    weapon.ReserveAmmo[0] = reserve;
    Utilities.SetStateChanged(weapon, "CBasePlayerWeapon", "m_iClip1");
    Utilities.SetStateChanged(weapon, "CBasePlayerWeapon", "m_pReserveAmmo");
  }
}