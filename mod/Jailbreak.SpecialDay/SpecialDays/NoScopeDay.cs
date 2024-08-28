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
  public static readonly FakeConVar<string> CV_WEAPON = new(
    "jb_sd_noscope_weapon",
    "Weapon to give to all players, recommended it be a weapon with a scope (duh)",
    "weapon_ssg08", ConVarFlags.FCVAR_NONE,
    new ItemValidator(allowMultiple: true));

  public static readonly FakeConVar<string> CV_WEAPON_WHITELIST = new(
    "jb_sd_noscope_allowedweapons",
    "Weapons to allow players to use, empty for no restrictions",
    string.Join(",",
      Tag.UTILITY.Union(new[] { "weapon_ssg08", "weapon_knife" }.ToHashSet())),
    ConVarFlags.FCVAR_NONE, new ItemValidator(allowMultiple: true));

  public static readonly FakeConVar<int> CV_KNIFE_DELAY = new(
    "jb_sd_noscope_knife_delay",
    "Time delay in seconds to give knives at, 0 to disable", 120,
    ConVarFlags.FCVAR_NONE, new RangeValidator<int>(0, 500));

  public static readonly FakeConVar<float> CV_GRAVITY =
    new("jb_sd_noscope_gravity",
      "Gravity to set during the special day, default is 800", 200f);

  public override SDType Type => SDType.NOSCOPE;

  public override ISDInstanceLocale Locale
    => new SoloDayLocale("No Scope",
      "Your scope broke! Fight against everyone else. No camping!");

  public override SpecialDaySettings Settings => new NoScopeSettings();

  public override void Setup() {
    if (CV_KNIFE_DELAY.Value > 0)
      Timers[CV_KNIFE_DELAY.Value] += () => {
        foreach (var player in PlayerUtil.GetAlive())
          player.GiveNamedItem("weapon_knife");
      };
    base.Setup();
  }

  public override void Execute() {
    foreach (var player in PlayerUtil.GetAlive()) {
      player.RemoveWeapons();
      foreach (var weapon in CV_WEAPON.Value.Split(","))
        player.GiveNamedItem(weapon);
    }

    Plugin.RegisterListener<Listeners.OnTick>(onTick);

    base.Execute();
  }

  private void onTick() {
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

    if (CV_WEAPON_WHITELIST.Value.Contains(activeWeapon.DesignerName,
      StringComparison.CurrentCultureIgnoreCase))
      return;
    activeWeapon.NextPrimaryAttackTick = Server.TickCount + 500;
  }

  override protected HookResult
    OnEnd(EventRoundEnd @event, GameEventInfo info) {
    var result = base.OnEnd(@event, info);
    Plugin.RemoveListener<Listeners.OnTick>(onTick);
    return result;
  }

  private class NoScopeSettings : FFASettings {
    public NoScopeSettings() {
      CtTeleport = TeleportType.RANDOM;
      TTeleport  = TeleportType.RANDOM;

      ConVarValues["sv_gravity"]       = CV_GRAVITY.Value;
      ConVarValues["sv_infinite_ammo"] = 2;
    }

    public override float FreezeTime(CCSPlayerController player) { return 1; }
  }
}