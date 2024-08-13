using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Jailbreak.English.SpecialDay;
using Jailbreak.Formatting.Views.SpecialDay;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;
using Jailbreak.Public.Utils;

namespace Jailbreak.SpecialDay.SpecialDays;

public class NoScopeDay(BasePlugin plugin, IServiceProvider provider)
  : FFADay(plugin, provider) {
  public override SDType Type => SDType.NOSCOPE;

  public override ISDInstanceLocale Locale
    => new SoloDayLocale("No Scope",
      "Your scope broke! Fight against everyone else. No camping!");

  public override SpecialDaySettings Settings => new NoScopeSettings();

  public override void Setup() {
    Timers[120] += () => {
      foreach (var player in PlayerUtil.GetAlive())
        player.GiveNamedItem("weapon_knife");
    };
    base.Setup();
  }

  public override void Execute() {
    foreach (var player in PlayerUtil.GetAlive()) {
      player.RemoveWeapons();
      player.GiveNamedItem("weapon_ssg08");
    }

    base.Execute();
  }

  override protected void OnTick() {
    foreach (var player in PlayerUtil.GetAlive()) disableScope(player);
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

    if (activeWeapon.DesignerName is "weapon_ssg08" or "weapon_knife") return;
    if (Tag.UTILITY.Contains(activeWeapon.DesignerName)) return;
    activeWeapon.NextPrimaryAttackTick = Server.TickCount + 500;
  }

  private class NoScopeSettings : FFASettings {
    public NoScopeSettings() {
      CtTeleport      = TeleportType.RANDOM;
      TTeleport       = TeleportType.RANDOM;
      RestrictWeapons = true;

      ConVarValues["sv_gravity"]       = (float)200;
      ConVarValues["sv_infinite_ammo"] = 2;
    }

    public override float FreezeTime(CCSPlayerController player) { return 1; }
  }
}