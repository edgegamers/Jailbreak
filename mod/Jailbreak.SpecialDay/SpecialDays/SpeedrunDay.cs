using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
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
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.SpecialDay.SpecialDays;

public class SpeedrunDay(BasePlugin plugin, IServiceProvider provider)
  : AbstractSpecialDay(plugin, provider), ISpecialDayMessageProvider {
  private static readonly int FIRST_SPEEDRUNNER_TIME = 20;
  private static readonly int FREEZE_TIME = 2;
  private static readonly int MAX_POINTS = 500;
  private readonly Random rng = new();

  private readonly IDictionary<int, ActivePlayerTrail<VectorTrailSegment>>
    activeTrails = new Dictionary<int, ActivePlayerTrail<VectorTrailSegment>>();

  private AbstractTrail<BeamTrailSegment>? bestTrail;
  private Timer? finishCheckTimer;

  /// <summary>
  ///   Negative values represent players who finished.
  ///   Positive values represent players who are still alive, and the value
  ///   being the distance they are from the target.
  /// </summary>
  private readonly IDictionary<int, float> finishTimestamps =
    new SortedDictionary<int, float>();

  private IGenericCommandNotifications generics = null!;
  private int round, playersAliveAtStart;
  private Timer? roundEndTimer;

  private float? roundStartTime;
  private Vector? start;
  private Vector? target;
  public override SDType Type => SDType.SPEEDRUN;

  private SpeedrunDayMessages msg => (SpeedrunDayMessages)Messages;

  public override SpecialDaySettings Settings => new SpeedrunSettings();
  public ISpecialDayInstanceMessages Messages => new SpeedrunDayMessages();

  private int Compare(int x, int y) {
    if (x < 0 && y < 0)
      return y.CompareTo(x); // Descending order for negative values
    if (x >= 0 && y >= 0)
      return x.CompareTo(y); // Ascending order for non-negative values
    return x < 0 ? -1 : 1;   // Negative values come first
  }

  public override void Setup() {
    generics = provider.GetRequiredService<IGenericCommandNotifications>();

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

    Timers[2] += () => msg.RunnerAssigned(speedrunner).ToAllChat();
    Timers[FIRST_SPEEDRUNNER_TIME - 30] +=
      () => msg.RuntimeLeft(30).ToAllChat();
    Timers[FIRST_SPEEDRUNNER_TIME] += () => {
      target = speedrunner.PlayerPawn.Value?.AbsOrigin!.Clone();
      startRound((int)Math.Ceiling(FIRST_SPEEDRUNNER_TIME * 1.1));

      finishCheckTimer = Plugin.AddTimer(0.03f, checkFinishers,
        TimerFlags.STOP_ON_MAPCHANGE | TimerFlags.REPEAT);
    };

    base.Setup();

    foreach (var player in PlayerUtil.GetAlive())
      player.SetColor(Color.FromArgb(32, 255, 255, 255));

    start = speedrunner.PlayerPawn.Value!.AbsOrigin!.Clone();
    speedrunner.UnFreeze();
    msg.YouAreRunner(FIRST_SPEEDRUNNER_TIME).ToPlayerChat(speedrunner);
    bestTrail = new ActiveBeamPlayerTrail(plugin, speedrunner, 0f,
      updateRate: 0.15f, maxPoints: MAX_POINTS);
  }

  private void startRound(int seconds) {
    roundStartTime = Server.CurrentTime;
    var alive = PlayerUtil.GetAlive().ToArray();
    playersAliveAtStart = PlayerUtil.GetAlive().Count();
    msg.BeginRound(++round, getEliminations(playersAliveAtStart), seconds)
     .ToAllChat();

    RoundUtil.SetTimeRemaining(seconds + FREEZE_TIME);

    foreach (var player in alive) {
      var pawn = player.PlayerPawn.Value;
      if (pawn == null) continue;
      pawn.Teleport(start);
      player.Freeze();
    }

    resetTrails();

    Plugin.AddTimer(FREEZE_TIME, () => {
      foreach (var player in PlayerUtil.GetAlive()) player.UnFreeze();
    }, TimerFlags.STOP_ON_MAPCHANGE);

    roundEndTimer = Plugin.AddTimer(seconds + FREEZE_TIME, endRound,
      TimerFlags.STOP_ON_MAPCHANGE);
  }

  private void checkFinishers() {
    if (target == null) return;
    foreach (var player in PlayerUtil.GetAlive()) {
      if (finishTimestamps.ContainsKey(player.Slot)) continue;
      var pos = player.PlayerPawn.Value?.AbsOrigin;
      if (pos == null) continue;
      var dist     = pos.DistanceSquared(target);
      var required = MathF.Pow(getRequiredDistance(), 2);
      if (dist >= required) continue;
      onFinish(player);
    }
  }

  private void onFinish(CCSPlayerController player) {
    finishTimestamps[player.Slot] = -Server.CurrentTime;
    var eliminations = getEliminations(PlayerUtil.GetAlive().Count());
    if (finishTimestamps.Count >= eliminations) endRound();
  }

  private void resetTrails() {
    bestTrail?.Kill();

    if (activeTrails.Count != 0 && finishTimestamps.Count != 0) {
      // Update the best trail
      var completers = finishTimestamps.Where(x => x.Value < 0).ToArray();
      if (completers.Length > 0) {
        // Of the players who finished, find the one who finished the fastest
        // since these times are negative timestamps, we want the "largest"
        // value (i.e. least negative)
        var bestPlayer = completers.MaxBy(x => x.Value).Key;
        activeTrails[bestPlayer].StopTracking();
        bestTrail = BeamTrail.FromTrail(plugin, activeTrails[bestPlayer]);
      }
    }

    foreach (var trail in activeTrails.Values) trail.Kill();

    activeTrails.Clear();

    foreach (var player in PlayerUtil.GetAlive())
      activeTrails[player.Slot] =
        new ActiveInvisiblePlayerTrail(plugin, player, 0f, MAX_POINTS);
  }

  // https://www.desmos.com/calculator/e1qwgpmtmz
  private float getRequiredDistance() {
    if (roundStartTime == null) return 0;
    var elapsedSeconds = (float)(Server.CurrentTime - roundStartTime);

    return 10 + elapsedSeconds + MathF.Pow(elapsedSeconds, 2.9f) / 5000;
  }

  private void endRound() {
    roundEndTimer?.Kill();
    if (target == null) {
      generics.Error("Target is null").ToAllChat();
      new EventRoundEnd(true).FireEvent(false);
      return;
    }

    var aliveCount          = PlayerUtil.GetAlive().Count();
    var playersDiedMidRound = playersAliveAtStart - aliveCount;
    var toEliminate         = getEliminations(aliveCount) - playersDiedMidRound;

    foreach (var player in PlayerUtil.GetAlive()) {
      if (finishTimestamps.ContainsKey(player.Slot)) continue;
      var dist = player.PlayerPawn.Value?.AbsOrigin?.Distance(target);
      if (dist == null) continue;
      finishTimestamps[player.Slot] = dist.Value;
    }

    announceTimes();
    // if (aliveCount <= 2) {
    //   // Announce winners, end the round, etc.
    //   // Maybe tp the loser to the winner and let the winner kill them
    //   return;
    // }

    if (toEliminate <= 0) {
      msg.NoneEliminated.ToAllChat();
      return;
    }

    var slowTimes = SlowestTimes(finishTimestamps);
    var fastTime = slowTimes.Select(s => s.Value)
     .LastOrDefault(FIRST_SPEEDRUNNER_TIME);
    var nextRoundTime     = (int)(Math.Ceiling(fastTime) + 10 - round * 2);
    var slowestEnumerator = SlowestTimes(finishTimestamps).GetEnumerator();

    for (var i = 0; i < toEliminate; i++) {
      if (!slowestEnumerator.MoveNext()) break;
      var (slot, _) = slowestEnumerator.Current;
      var player = Utilities.GetPlayerFromSlot(slot);
      if (player == null || !player.IsValid) continue;
      player.CommitSuicide(false, true);
      msg.PlayerEliminated(player).ToAllChat();
    }

    slowestEnumerator.Dispose();

    Plugin.AddTimer(3f, () => { startRound(nextRoundTime); },
      TimerFlags.STOP_ON_MAPCHANGE);
  }


  private int getEliminations(int players) {
    return players switch {
      <= 4  => 1,
      <= 8  => 2,
      <= 12 => 3,
      _     => 4
    };
  }

  private void announceTimes() {
    var times = finishTimestamps.OrderBy(x => x.Value).ToArray();
    for (var i = 0; i < times.Length; i++) {
      var (slot, time) = times[i];
      var player = Utilities.GetPlayerFromSlot(slot);
      if (player == null) continue;
      msg.PlayerTime(player, i + 1, time).ToAllChat();
    }
  }

  public static IEnumerable<KeyValuePair<int, float>>
    SlowestTimes(IDictionary<int, float> timeDistances) {
    return timeDistances.OrderByDescending(entry => entry.Value >= 0)
     .ThenByDescending(entry => entry.Value >= 0 ? entry.Value : float.MinValue)
     .ThenBy(entry => entry.Value < 0 ? entry.Value : float.MaxValue);
  }

  public override HookResult OnEnd(EventRoundEnd @event, GameEventInfo info) {
    var result = base.OnEnd(@event, info);

    finishCheckTimer?.Kill();
    bestTrail?.Kill();

    foreach (var trail in activeTrails.Values) trail.Kill();

    activeTrails.Clear();

    return result;
  }

  public class SpeedrunSettings : SpecialDaySettings {
    public SpeedrunSettings() {
      CtTeleport = TeleportType.ARMORY_STACKED;
      TTeleport = TeleportType.ARMORY_STACKED;
      RestrictWeapons = true;
      StripToKnife = true;
      ConVarValues["mp_ignore_round_win_conditions"] = true;
      WithFriendlyFire();
    }

    public override Func<int> RoundTime => () => FIRST_SPEEDRUNNER_TIME;

    public override ISet<string>? AllowedWeapons(CCSPlayerController player) {
      // Return empty set to allow no weapons
      return new HashSet<string>();
    }

    public override float FreezeTime(CCSPlayerController player) { return 3; }
  }
}