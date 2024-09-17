using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using Jailbreak.English.SpecialDay;
using Jailbreak.Formatting.Views.SpecialDay;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;
using Jailbreak.Public.Utils;
using Jailbreak.Validator;

namespace Jailbreak.SpecialDay.SpecialDays;

public class OneInTheChamberDay(BasePlugin plugin, IServiceProvider provider)
  : FFADay(plugin, provider) {
  public static readonly FakeConVar<string> CV_WEAPON = new("jb_sd_oitc_weapon",
    "Weapon to give to players for the day", "weapon_deagle",
    ConVarFlags.FCVAR_NONE, new ItemValidator(WeaponType.GUNS));

  public static readonly FakeConVar<string> CV_ADDITIONAL_WEAPON = new(
    "jb_sd_oitc_additionalweapon",
    "Additional (non-ammo restricted) weapons to give for the day",
    "weapon_knife", ConVarFlags.FCVAR_NONE, new ItemValidator());

  private bool started;
  public override SDType Type => SDType.OITC;

  public override ISDInstanceLocale Locale
    => new SoloDayLocale("One in the Chamber", "You only have one bullet.",
      "Kill someone to get another bullet.", "One-Hit-Kills! No camping!");

  public override SpecialDaySettings Settings => new OitcSettings();

  public override void Setup() {
    base.Setup();
    Plugin.RegisterEventHandler<EventItemPickup>(OnPickup);
    Plugin.RegisterEventHandler<EventPlayerHurt>(OnPlayerDamage);
    Plugin.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
  }

  public override void Execute() {
    base.Execute();

    foreach (var player in PlayerUtil.GetAlive()) {
      player.RemoveWeapons();
      if (CV_ADDITIONAL_WEAPON.Value.Length > 0)
        player.GiveNamedItem(CV_ADDITIONAL_WEAPON.Value);
      if (CV_WEAPON.Value.Length > 0) {
        player.GiveNamedItem(CV_WEAPON.Value);
        player.GetWeaponBase(CV_WEAPON.Value)?.SetAmmo(1, 0);
      }
    }

    started = true;
  }

  protected HookResult OnPickup(EventItemPickup @event, GameEventInfo info) {
    if (!started) return HookResult.Continue;

    var player = @event.Userid;
    if (player == null || !player.IsValid) return HookResult.Continue;
    player.RemoveWeapons();
    player.SetHealth(1);
    return HookResult.Continue;
  }

  private HookResult
    OnPlayerDamage(EventPlayerHurt @event, GameEventInfo info) {
    if (@event.Userid == null || !@event.Userid.IsValid)
      return HookResult.Continue;
    if (@event.Attacker == null || !@event.Attacker.IsValid)
      return HookResult.Continue;
    @event.Userid?.SetHealth(0);
    return HookResult.Changed;
  }

  private HookResult
    OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info) {
    if (@event.Attacker == null) return HookResult.Continue;
    @event.Attacker.GetWeaponBase(CV_WEAPON.Value)?.AddBulletsToMagazine(1);
    return HookResult.Continue;
  }

  override protected HookResult
    OnEnd(EventRoundEnd @event, GameEventInfo info) {
    Plugin.DeregisterEventHandler<EventPlayerHurt>(OnPlayerDamage);
    Plugin.DeregisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
    Plugin.DeregisterEventHandler<EventItemPickup>(OnPickup);
    return base.OnEnd(@event, info);
  }

  public class OitcSettings : SpecialDaySettings {
    public OitcSettings() {
      CtTeleport = TeleportType.RANDOM;
      TTeleport  = TeleportType.RANDOM;
      WithFriendlyFire();

      ConVarValues["mp_death_drop_gun"] = 0;
    }

    public override ISet<string> AllowedWeapons(CCSPlayerController player) {
      return new HashSet<string> {
        CV_WEAPON.Value, CV_ADDITIONAL_WEAPON.Value
      };
    }
  }
}