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
using Jailbreak.Public.Mod.Trail;
using Jailbreak.Public.Utils;
using Jailbreak.Trail;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic.CompilerServices;

namespace Jailbreak.SpecialDay.SpecialDays;

public class SpeedrunDay(BasePlugin plugin, IServiceProvider provider)
  : AbstractSpecialDay(plugin, provider), ISpecialDayMessageProvider {
  private IGenericCommandNotifications generics;
  private readonly Random rng = new();
  private Vector? target;
  public override SDType Type => SDType.SPEEDRUN;

  private SpeedrunDayMessages msg => (SpeedrunDayMessages)Messages;

  private ActivePlayerTrail<BeamTrailSegment> bestTrail;
  private int round = 0;
  private Vector start;

  private IDictionary<int, ActivePlayerTrail<VectorTrailSegment>> activeTrails =
    new Dictionary<int, ActivePlayerTrail<VectorTrailSegment>>();

  private IDictionary<int, float>
    raceTimes = new SortedDictionary<int, float>();

  public override SpecialDaySettings Settings => new SpeedrunSettings();
  public ISpecialDayInstanceMessages Messages => new SpeedrunDayMessages();

  public override void Setup() {
    generics = provider.GetRequiredService<IGenericCommandNotifications>();

    // Timers[60] 

    var speedrunner = PlayerUtil.GetRandomFromTeam(rng.Next(2) == 0 ?
      CsTeam.Terrorist :
      CsTeam.CounterTerrorist);

    if (speedrunner == null) {
      speedrunner = PlayerUtil.GetAlive().FirstOrDefault();
      if (speedrunner == null) {
        generics.Error("Could not find a valid speedrunner").ToAllChat();
        RoundUtil.SetTimeRemaining(1);
        return;
      }
    }

    Timers[2]  += () => msg.RunnerAssigned(speedrunner).ToAllChat();
    Timers[3]  += () => msg.RuntimeLeft(30).ToAllChat();
    Timers[10] += () => startRound(60, PlayerUtil.GetAlive().Count() / 4);

    base.Setup();

    start = speedrunner.PlayerPawn.Value!.AbsOrigin!.Clone();
    speedrunner.UnFreeze();
    msg.YouAreRunner(60).ToPlayerChat(speedrunner);
    bestTrail = new ActiveBeamPlayerTrail(plugin, speedrunner,
      updateRate: 0.15f, maxPoints: 10);
    speedrunner.SetColor(Color.DodgerBlue);
  }

  private void startRound(int seconds, int eliminations) {
    msg.BeginRound(seconds, eliminations).ToAllChat();

    foreach (var player in PlayerUtil.GetAlive()) {
      var pawn = player.PlayerPawn.Value;
      if (pawn == null) continue;
      pawn.Teleport(start);
    }
  }

  private void resetTrails() {
    if (activeTrails.Count != 0 && raceTimes.Count != 0) {
      // Update the best trail
      var bestPlayer = raceTimes.MinBy(x => x.Value).Key;
      var bestTrail  = activeTrails[bestPlayer];
    }
  }

  private void endRound(int eliminations) { }

  private void announceTimes() {
    var times = raceTimes.OrderBy(x => x.Value).ToArray();
    for (var i = 0; i < times.Length; i++) {
      var (slot, time) = times[i];
      var place = i + 1;
      var suffix = place == 1 ? "st" :
        place == 2 ? "nd" :
        place == 3 ? "rd" : "th";
      var player = Utilities.GetPlayerFromSlot(slot);
      if (player == null) continue;
      msg.PlayerTime(player, i + 1, time).ToAllChat();
    }
  }

  public override HookResult OnEnd(EventRoundEnd @event, GameEventInfo info) {
    var result = base.OnEnd(@event, info);

    bestTrail.Kill();
    foreach (var trail in activeTrails.Values) { trail.Kill(); }

    return result;
  }

  public class SpeedrunSettings : SpecialDaySettings {
    public SpeedrunSettings() {
      CtTeleport      = TeleportType.ARMORY_STACKED;
      TTeleport       = TeleportType.ARMORY_STACKED;
      RestrictWeapons = true;
      StripToKnife    = true;
    }

    public override ISet<string>? AllowedWeapons(CCSPlayerController player) {
      // Return empty set to allow no weapons
      return new HashSet<string>();
    }

    public override float FreezeTime(CCSPlayerController player) { return 3; }
  }
}