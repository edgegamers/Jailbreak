using CounterStrikeSharp.API.Core;
using Jailbreak.English.SpecialDay;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views.SpecialDay;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;
using Jailbreak.Public.Utils;

namespace Jailbreak.SpecialDay.SpecialDays;

public class HEDay(BasePlugin plugin, IServiceProvider provider)
  : AbstractSpecialDay(plugin, provider), ISpecialDayMessageProvider {
  public override SDType Type => SDType.HE;
  public override SpecialDaySettings Settings => new HESettings();

  public ISDInstanceLocale Locale
    => new SoloDayLocale("HE Only",
      "Grenades Only—No guns. Fight against everyone else. No Camping!");

  public override void Setup() {
    Plugin.RegisterEventHandler<EventGrenadeThrown>(onThrow);
    Timers[10] += () => Locale.BeginsIn(10).ToAllChat();
    Timers[15] += () => Locale.BeginsIn(5).ToAllChat();
    Timers[20] += Execute;
    
    base.Setup();
  }

  public override void Execute() {
    foreach (var player in PlayerUtil.GetAlive()) 
      player.GiveNamedItem("weapon_hegrenade");
    base.Execute();
    Locale.BeginsIn(0).ToAllChat();
  }

  private HookResult onThrow(EventGrenadeThrown @event, GameEventInfo info) {
    var player = @event.Userid;
    if (player == null || !player.IsReal() || !player.PawnIsAlive)
      return HookResult.Continue;
    player.GiveNamedItem("weapon_hegrenade");
    return HookResult.Continue;
  }
  
  override protected HookResult
    OnEnd(EventRoundEnd @event, GameEventInfo info) {
    var result = base.OnEnd(@event, info);
    Plugin.DeregisterEventHandler<EventGrenadeThrown>(onThrow);
    return result;
  }

  public class HESettings : SpecialDaySettings {
    public HESettings() {
      CtTeleport   = TeleportType.RANDOM;
      TTeleport    = TeleportType.RANDOM;
      StripToKnife = true;
      WithFriendlyFire();
    }
    
    public override float FreezeTime(CCSPlayerController player) { return 1; }

    public override ISet<string>? AllowedWeapons(CCSPlayerController player) {
      return new HashSet<string> {"weapon_hegrenade"};
    }
  }
}