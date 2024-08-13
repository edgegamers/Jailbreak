using CounterStrikeSharp.API.Core;
using Jailbreak.English.SpecialDay;
using Jailbreak.Formatting.Views.SpecialDay;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;
using Jailbreak.Public.Utils;

namespace Jailbreak.SpecialDay.SpecialDays;

public class OneInTheChamberDay(BasePlugin plugin, IServiceProvider provider)
  : FFADay(plugin, provider) {
  public override SDType Type => SDType.OITC;

  public override ISDInstanceLocale Locale
    => new SoloDayLocale("One in the Chamber", "You only have one bullet.",
      "Kill someone to get another bullet.", "One-Hit-Kills! No camping!");

  public override SpecialDaySettings Settings => new OitcSettings();

  public override void Setup() {
    base.Setup();
    Plugin.RegisterEventHandler<EventPlayerHurt>(OnPlayerDamage);
    Plugin.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
  }

  public override void Execute() {
    base.Execute();

    foreach (var player in PlayerUtil.GetAlive()) {
      player.RemoveWeapons();
      player.GiveNamedItem("weapon_knife");
      player.GiveNamedItem("weapon_deagle");
      player.GetWeaponBase("weapon_deagle")?.SetAmmo(1, 0);
    }
  }

  private HookResult
    OnPlayerDamage(EventPlayerHurt @event, GameEventInfo info) {
    if (@event.Userid == null || !@event.Userid.IsValid)
      return HookResult.Continue;
    @event.Userid?.SetHealth(0);
    return HookResult.Changed;
  }

  private HookResult
    OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info) {
    if (@event.Attacker == null) return HookResult.Continue;
    @event.Attacker.GetWeaponBase("weapon_deagle")?.AddBulletsToMagazine(1);
    return HookResult.Continue;
  }

  override protected HookResult
    OnEnd(EventRoundEnd @event, GameEventInfo info) {
    Plugin.DeregisterEventHandler<EventPlayerHurt>(OnPlayerDamage);
    Plugin.DeregisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
    return base.OnEnd(@event, info);
  }
}

public class OitcSettings : SpecialDaySettings {
  public OitcSettings() {
    CtTeleport      = TeleportType.RANDOM;
    TTeleport       = TeleportType.RANDOM;
    RestrictWeapons = true;
    WithFriendlyFire();

    ConVarValues["mp_death_drop_gun"] = 0;
  }

  public override ISet<string> AllowedWeapons(CCSPlayerController player) {
    return new HashSet<string> { "weapon_deagle", "weapon_knife" };
  }
}