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

public class InfectionDay(BasePlugin plugin, IServiceProvider provider)
  : ArmoryRestrictedDay(plugin, provider, CsTeam.CounterTerrorist),
    ISpecialDayMessageProvider {
  public override SDType Type => SDType.INFECTION;

  public override SpecialDaySettings Settings => new InfectionSettings();
  private readonly ICollection<ulong> swappedPrisoners = new HashSet<ulong>();

  public class InfectionSettings : SpecialDaySettings {
    public InfectionSettings() {
      CtTeleport      = TeleportType.ARMORY;
      TTeleport       = TeleportType.ARMORY;
      RestrictWeapons = true;

      WithRespawns(CsTeam.CounterTerrorist);
    }

    public override ISet<string>? AllowedWeapons(CCSPlayerController player) {
      if (player.Team == CsTeam.Terrorist)
        return Tag.UTILITY.Union(Tag.PISTOLS).ToHashSet();
      return null;
    }

    public override float FreezeTime(CCSPlayerController player) {
      return player.Team == CsTeam.CounterTerrorist ? 5 : 2;
    }

    public override int InitialHealth(CCSPlayerController player) {
      return player.Team == CsTeam.CounterTerrorist ? 50 : 200;
    }
  }

  public override void Setup() {
    Timers[15] += () => Messages.BeginsIn(15).ToAllChat();
    Timers[30] += Execute;
    base.Setup();
    plugin.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
  }

  private HookResult
    OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info) {
    var player = @event.Userid;
    if (player == null || !player.IsValid) return HookResult.Continue;

    if (player.Team != CsTeam.Terrorist) return HookResult.Continue;

    var pos = player.PlayerPawn.Value?.AbsOrigin;
    if (pos == null) return HookResult.Continue;
    swappedPrisoners.Add(player.SteamID);
    player.Respawn();
    player.RemoveWeapons();

    plugin.AddTimer(3, () => { player.GiveNamedItem("weapon_knife"); });

    var nearest = PlayerUtil.GetAlive()
     .Where(p
        => p.PlayerPawn.Value != null && p.PlayerPawn.Value.AbsOrigin != null)
     .ToList();

    nearest.Sort((a, b)
      => a.PlayerPawn.Value!.AbsOrigin!.DistanceSquared(pos)
      <= b.PlayerPawn.Value!.AbsOrigin!.DistanceSquared(pos) ?
        -1 :
        1);
    msg.YouWereInfectedMessage(@event.Attacker);
    if (nearest.Count == 0) {
      player.PlayerPawn.Value!.Teleport(pos);
      return HookResult.Continue;
    }

    var target = nearest.First();
    player.PlayerPawn.Value!.Teleport(target.PlayerPawn.Value!.AbsOrigin!);
    if (target.Team == CsTeam.Terrorist)
      msg.InfectedWarning(player).ToPlayerChat(target);
    return HookResult.Continue;
  }

  public override HookResult OnEnd(EventRoundEnd @event, GameEventInfo info) {
    var result = base.OnEnd(@event, info);
    foreach (var steam in swappedPrisoners) {
      var player = Utilities.GetPlayerFromSteamId(steam);
      if (player == null) continue;
      player.ChangeTeam(CsTeam.Terrorist);
    }

    return result;
  }

  public ISpecialDayInstanceMessages Messages => new InfectionDayMessages();
  private InfectionDayMessages msg => (InfectionDayMessages)Messages;
}