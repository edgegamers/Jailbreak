using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using Jailbreak.English.SpecialDay;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;
using Jailbreak.Public.Utils;

namespace Jailbreak.SpecialDay.SpecialDays;

public class OneInTheChamberDay(BasePlugin plugin, IServiceProvider provider) 
  : FFADay(plugin, provider) {
  
  public override SDType Type => SDType.OITC;
  
  public override ISpecialDayInstanceMessages Messages
    => new SoloDayMessages("One In The Chamber",
      "You only have one bullet.", " Kill someone else to get another bullet.",
      "No camping!");

  public override SpecialDaySettings Settings => new OitcSettings();
  
  public override void Setup() {
    plugin.RegisterEventHandler<EventPlayerHurt>(OnPlayerDamage);
    plugin.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
    base.Setup();
  }
  
  public override void Execute() {
    foreach (var player in PlayerUtil.GetAlive()) {
      player.RemoveWeapons();
      player.GiveNamedItem("weapon_knife");
      player.GiveNamedItem(CsItem.Deagle);
      player.GetWeaponBase("weapon_deagle").SetAmmo(1, 0);
    }
    base.Execute();
  }

  private HookResult OnPlayerDamage(EventPlayerHurt @event, GameEventInfo info) 
  {
    var  player     = @event.Userid!;
    bool usedDeagle = @event.Weapon == "weapon_deagle";
    if (!player.IsValid || !usedDeagle) return HookResult.Continue;
    
    player.SetHealth(0);
    return HookResult.Changed;
  }

  private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info) {
    if (@event.Attacker == null) return HookResult.Continue;
    @event.Attacker.GetWeaponBase("weapon_deagle").AddBulletsToMagazine(1);
    return HookResult.Continue;
  }

  public override HookResult OnEnd(EventRoundEnd @event, GameEventInfo info) {
    plugin.DeregisterEventHandler<EventPlayerHurt>(OnPlayerDamage);
    plugin.DeregisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
    return HookResult.Continue;
  }
}

public class OitcSettings : SpecialDaySettings {
  public OitcSettings() {
    CtTeleport      = TeleportType.ARMORY;
    TTeleport       = TeleportType.ARMORY;
    RestrictWeapons = true;

    ConVarValues["mp_death_drop_gun"] = false;
  }
}