using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using Jailbreak.English.SpecialDay;
using Jailbreak.Formatting.Views.SpecialDay;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;
using Jailbreak.Public.Utils;
using Jailbreak.Validator;

namespace Jailbreak.SpecialDay.SpecialDays;

public class NoScopeDay(BasePlugin plugin, IServiceProvider provider)
  : FFADay(plugin, provider) {
  public override SDType Type => SDType.NOSCOPE;

  public override ISDInstanceLocale Locale
    => new SoloDayLocale("No Scope",
      "Your scope broke! Fight against everyone else. No camping!");

  public override SpecialDaySettings Settings => new NoScopeSettings(this);

  public readonly FakeConVar<string> CvWeapon = new("jb_sd_noscope_weapon",
    "Weapon to give to all players, recommended it be a weapon with a scope (duh)",
    "weapon_ssg08", ConVarFlags.FCVAR_NONE,
    new ItemValidator(allowMultiple: true));

  public readonly FakeConVar<string> CvWeaponWhitelist = new(
    "jb_sd_noscope_allowedweapons",
    "Weapons to allow players to use, empty for no restrictions",
    string.Join(",",
      Tag.UTILITY.Union(new[] { "weapon_ssg08", "weapon_knife" }.ToHashSet())),
    ConVarFlags.FCVAR_NONE, new ItemValidator(allowMultiple: true));

  public readonly FakeConVar<int> CvKnifeDelay = new(
    "jb_sd_noscope_knife_delay",
    "Time delay in seconds to give knives at, 0 to disable", 120,
    ConVarFlags.FCVAR_NONE, new RangeValidator<int>(0, 500));

  public readonly FakeConVar<float> CvGravity = new("jb_sd_noscope_gravity",
    "Gravity to set during the special day, default is 800", 200f);

  public override void Setup() {
    if (CvKnifeDelay.Value > 0)
      Timers[CvKnifeDelay.Value] += () => {
        foreach (var player in PlayerUtil.GetAlive())
          player.GiveNamedItem("weapon_knife");
      };
    base.Setup();
  }

  public override void Execute() {
    foreach (var player in PlayerUtil.GetAlive()) {
      player.RemoveWeapons();
      foreach (var weapon in CvWeapon.Value.Split(","))
        player.GiveNamedItem(weapon);
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

    if (CvWeaponWhitelist.Value.Contains(activeWeapon.DesignerName,
      StringComparison.CurrentCultureIgnoreCase))
      return;
    activeWeapon.NextPrimaryAttackTick = Server.TickCount + 500;
  }

  private class NoScopeSettings : FFASettings {
    public NoScopeSettings(NoScopeDay day) {
      CtTeleport      = TeleportType.RANDOM;
      TTeleport       = TeleportType.RANDOM;
      RestrictWeapons = true;

      ConVarValues["sv_gravity"]       = day.CvGravity.Value;
      ConVarValues["sv_infinite_ammo"] = 2;
    }

    public override float FreezeTime(CCSPlayerController player) { return 1; }
  }
}