using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.English.SpecialDay;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;
using Jailbreak.Public.Utils;

namespace Jailbreak.SpecialDay.SpecialDays;

public class InfectionDay : AbstractArmoryRestrictedDay,
  ISpecialDayMessageProvider {
  private readonly ICollection<int> swappedPrisoners = new HashSet<int>();

  public InfectionDay(BasePlugin Plugin, IServiceProvider provider) : base(
    Plugin, provider, CsTeam.CounterTerrorist) { }

  public override SDType Type => SDType.INFECTION;

  public override SpecialDaySettings Settings => new InfectionSettings();
  private InfectionDayLocale msg => (InfectionDayLocale)Locale;

  public ISDInstanceLocale Locale => new InfectionDayLocale();

  public override void Setup() {
    Timers[15] += () => {
      Locale.BeginsIn(15).ToAllChat();
      msg.DamageWarning(5).ToAllChat();
    };
    Timers[20] += () => {
      foreach (var t in PlayerUtil.FromTeam(CsTeam.Terrorist)) EnableDamage(t);
    };
    Timers[30] += Execute;
    base.Setup();

    foreach (var ct in PlayerUtil.FromTeam(CsTeam.CounterTerrorist))
      ct.SetColor(Color.LimeGreen);

    Plugin.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
    Plugin.RegisterEventHandler<EventPlayerSpawn>(OnRespawn);
  }

  private HookResult
    OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info) {
    var player = @event.Userid;
    if (player == null || !player.IsValid) return HookResult.Continue;
    if (player.Team != CsTeam.Terrorist) return HookResult.Continue;

    var pos = player.PlayerPawn.Value?.AbsOrigin?.Clone();
    if (pos == null) return HookResult.Continue;

    if (PlayerUtil.FromTeam(CsTeam.Terrorist).Count() == 1)
      return HookResult.Continue;

    var nearest = PlayerUtil.GetAlive()
     .Where(p => p.Index != player.Index)
     .Where(p
        => p.PlayerPawn.Value != null && p.PlayerPawn.Value.AbsOrigin != null)
     .ToList();

    nearest.Sort((a, b)
      => a.PlayerPawn.Value!.AbsOrigin!.DistanceSquared(pos)
      <= b.PlayerPawn.Value!.AbsOrigin!.DistanceSquared(pos) ?
        -1 :
        1);

    var target = nearest.FirstOrDefault();
    if (target != null && target.Team == CsTeam.Terrorist)
      msg.InfectedWarning(player).ToChat(target);

    var tpSpot = target != null ?
      target.PlayerPawn.Value!.AbsOrigin!.Clone() :
      pos;

    swappedPrisoners.Add(player.Slot);
    if (!player.IsValid) return HookResult.Continue;

    msg.YouWereInfectedMessage(
        @event.Attacker != null && @event.Attacker.IsValid ?
          @event.Attacker :
          null)
     .ToChat(player);
    Plugin.AddTimer(0.1f, () => {
      player.SwitchTeam(CsTeam.CounterTerrorist);
      player.Respawn();
      Plugin.AddTimer(0.1f, () => {
        player.RemoveWeapons();
        Plugin.AddTimer(3, () => { player.GiveNamedItem("weapon_knife"); });
      });
      if (nearest.Count == 0 || target == null) {
        player.PlayerPawn.Value!.Teleport(pos);
        return;
      }

      player.PlayerPawn.Value!.Teleport(tpSpot);
    });
    return HookResult.Continue;
  }

  public HookResult OnRespawn(EventPlayerSpawn @event, GameEventInfo info) {
    var player = @event.Userid;
    if (player == null || !player.IsValid) return HookResult.Continue;
    if (player.Team != CsTeam.CounterTerrorist) return HookResult.Continue;

    var hp = Settings.InitialHealth(player);
    if (hp != -1) Plugin.AddTimer(0.1f, () => { player.SetHealth(hp); });

    var color = swappedPrisoners.Contains(player.Slot) ?
      Color.DarkOliveGreen :
      Color.ForestGreen;
    player.SetColor(color);
    return HookResult.Continue;
  }

  override protected HookResult
    OnEnd(EventRoundEnd @event, GameEventInfo info) {
    var result = base.OnEnd(@event, info);
    Plugin.DeregisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
    Plugin.DeregisterEventHandler<EventPlayerSpawn>(OnRespawn);

    foreach (var index in swappedPrisoners) {
      var player = Utilities.GetPlayerFromSlot(index);
      if (player == null) continue;
      player.SwitchTeam(CsTeam.Terrorist);
    }

    return result;
  }

  public class InfectionSettings : SpecialDaySettings {
    public InfectionSettings() {
      CtTeleport      = TeleportType.ARMORY;
      TTeleport       = TeleportType.RANDOM;
      RestrictWeapons = true;

      WithRespawns(CsTeam.CounterTerrorist);
    }

    public override ISet<string>? AllowedWeapons(CCSPlayerController player) {
      return player.Team == CsTeam.CounterTerrorist ?
        Tag.UTILITY.Union(Tag.PISTOLS).ToHashSet() :
        null;
    }

    public override float FreezeTime(CCSPlayerController player) {
      return player.Team == CsTeam.CounterTerrorist ? 6 : 2;
    }

    public override int InitialHealth(CCSPlayerController player) {
      return player.Team == CsTeam.Terrorist ? 75 : 200;
    }
  }
}