using CounterStrikeSharp.API.Core;
using Jailbreak.English.SpecialDay;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;

namespace Jailbreak.SpecialDay.SpecialDays;

public class TeleportDay(BasePlugin plugin, IServiceProvider provider)
  : FFADay(plugin, provider), ISpecialDayMessageProvider {
  public override SDType Type => SDType.FFA;
  public override SpecialDaySettings Settings => new TeleportSettings();

  public override ISpecialDayInstanceMessages Messages
    => new SoloDayMessages("Teleport",
      "Free for all! If you damage someone, you will swap places with them!");

  public override void Setup() {
    base.Setup();
    Plugin.RegisterEventHandler<EventPlayerHurt>(onDamage);
  }

  private HookResult onDamage(EventPlayerHurt @event, GameEventInfo info) {
    var player = @event.Userid;
    if (player == null || !player.IsValid) return HookResult.Continue;
    var attacker = @event.Attacker;
    if (attacker == null || !attacker.IsValid) return HookResult.Continue;

    var playerLoc   = player.Pawn.Value?.AbsOrigin;
    var attackerLoc = attacker.Pawn.Value?.AbsOrigin;

    if (playerLoc == null || attackerLoc == null) return HookResult.Continue;

    var tmp = playerLoc.Clone();

    player.Pawn.Value?.Teleport(attackerLoc);
    attacker.Pawn.Value?.Teleport(tmp);
    return HookResult.Continue;
  }

  override protected HookResult
    OnEnd(EventRoundEnd @event, GameEventInfo info) {
    Plugin.DeregisterEventHandler<EventPlayerHurt>(onDamage);
    return base.OnEnd(@event, info);
  }

  public class TeleportSettings : SpecialDaySettings {
    private readonly Random rng;

    public TeleportSettings() {
      CtTeleport   = TeleportType.ARMORY;
      TTeleport    = TeleportType.ARMORY;
      StripToKnife = false;
      rng          = new Random();
      WithFriendlyFire();
    }

    public override float FreezeTime(CCSPlayerController player) {
      return rng.NextSingle() * 5 + 2;
    }
  }
}