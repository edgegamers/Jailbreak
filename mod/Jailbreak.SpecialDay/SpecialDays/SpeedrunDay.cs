using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.English.SpecialDay;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Draw;
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
  private const int MAX_POINTS = 500;

  public static readonly FakeConVar<int> CvInitialSpeedrunTime =
    new("css_jb_speedrun_initial_time",
      "Duration in seconds to grant the speedrunner", 40);

  public static readonly FakeConVar<int> CvFirstRoundFreeze =
    new("css_jb_speedrun_CvFirstRoundFreeze.Value",
      "Duration in seconds to give players time to read the rules of speedrun",
      8);

  public static readonly FakeConVar<int> CvFreezeTime =
    new("css_jb_speedrun_CvFreezeTime.Value",
      "Duration in seconds to freeze players before the speedrun starts", 2);

  private readonly Dictionary<int, ActivePlayerTrail<VectorTrailSegment>>
    activeTrails = new();

  /// <summary>
  ///   Negative values represent players who finished.
  ///   Positive values represent players who are still alive, and the value
  ///   being the distance they are from the target.
  /// </summary>
  private readonly SortedDictionary<int, float> finishTimestamps = new();

  private readonly Random rng = new();
  private float? bestTime;
  private int? bestTimePlayerSlot;

  private AbstractTrail<BeamTrailSegment>? bestTrail;
  private Timer? finishCheckTimer;

  private IGenericCmdLocale generics = null!;
  private int round, playersAliveAtStart;
  private Timer? roundEndTimer;

  private float? roundStartTime;
  private Vector? start;
  private Vector? target;
  private BeamCircle? targetCircle;
  private CCSPlayerController? speedrunner;

  private bool isRoundActive
    => provider.GetRequiredService<ISpecialDayManager>().CurrentSD == this;

  public override SDType Type => SDType.SPEEDRUN;

  public ISDInstanceLocale Locale => new SpeedrunDayLocale();

  public override SpecialDaySettings Settings => new SpeedrunSettings();

  public override void Setup() {
    generics = Provider.GetRequiredService<IGenericCmdLocale>();

    foreach (var player in Utilities.GetPlayers()
     .Where(p => p is {
        PawnIsAlive: false, Team: CsTeam.Terrorist or CsTeam.CounterTerrorist
      }))
      player.Respawn();

    speedrunner = getRunner();

    if (speedrunner == null) {
      speedrunner = PlayerUtil.GetAlive().FirstOrDefault();
      if (speedrunner == null) {
        panic("Could not find a speedrunner");
        return;
      }
    }

    Timers[0.1f] += () => {
      // Needed since players who respawned are given knife later
      foreach (var player in PlayerUtil.GetAlive()) {
        player.RemoveWeapons();
        player.SetColor(Color.FromArgb(100, 255, 255, 255));
      }
    };
    Timers[CvFirstRoundFreeze.Value - 4] += () => {
      if (!speedrunner.IsValid) speedrunner = getRunner();
      if (speedrunner == null) {
        panic("Speedrunner is invalid, and we cannot find a new one");
        return;
      }

      Locale.RunnerAssigned(speedrunner).ToAllChat();
      speedrunner.SetColor(Color.DodgerBlue);
      Locale.YouAreRunner(CvInitialSpeedrunTime.Value).ToChat(speedrunner);
    };
    Timers[CvFirstRoundFreeze.Value] += () => {
      if (!speedrunner.IsValid) {
        speedrunner = getRunner();
        if (speedrunner == null) {
          panic(
            "Original speedrunner is invalid, and we cannot find a new one");
          return;
        }

        speedrunner.SetColor(Color.DodgerBlue);
        Locale.RunnerReassigned(speedrunner).ToAllChat();
        Locale.YouAreRunner(CvInitialSpeedrunTime.Value).ToChat(speedrunner);
      }

      start = speedrunner.PlayerPawn.Value!.AbsOrigin!.Clone();
      speedrunner.UnFreeze();
      bestTrail = createFirstTrail(speedrunner);
    };

    Timers[CvInitialSpeedrunTime.Value + CvFirstRoundFreeze.Value - 30] += ()
      => {
      if (!speedrunner.IsValid) speedrunner = getRunner();
      if (speedrunner == null) {
        panic("Original speedrunner is invalid, and we cannot find a new one");
        return;
      }

      Locale.RuntimeLeft(30).ToChat(speedrunner);
    };

    Timers[CvInitialSpeedrunTime.Value + CvFirstRoundFreeze.Value - 10] += ()
      => Locale.RuntimeLeft(10).ToChat(speedrunner);
    Timers[CvInitialSpeedrunTime.Value + CvFirstRoundFreeze.Value] += () => {
      target = speedrunner.Pawn.Value?.AbsOrigin;
      if (target == null) {
        panic("Could not get AbsOrigin of speedrunner");
        return;
      }

      target       = target.Clone();
      targetCircle = new BeamCircle(Plugin, target!, 10, 16);
      targetCircle.SetColor(Color.Green);
      targetCircle.Draw();

      if (bestTrail is null) {
        generics.Error("bestTrail is null").ToAllChat();
        return;
      }

      if (bestTrail is ActivePlayerTrail<BeamTrailSegment> active)
        active.StopTracking();

      var timeSpent = bestTrail.GetEndSegment()!.GetSpawnTime()
        - bestTrail.GetStartSegment()!.GetSpawnTime();

      bestTime = timeSpent;

      var minTime = CvInitialSpeedrunTime.Value * 0.5;

      startRound((int)Math.Ceiling(Math.Max(timeSpent * 1.1, minTime)));

      finishCheckTimer = Plugin.AddTimer(0.03f, checkFinishers,
        TimerFlags.STOP_ON_MAPCHANGE | TimerFlags.REPEAT);
    };

    base.Setup();
  }

  private ActivePlayerTrail<BeamTrailSegment> createFirstTrail(
    CCSPlayerController player) {
    var trail = new ActivePulsatingBeamPlayerTrail(Plugin, player, 0f,
      MAX_POINTS, 0.15f);
    trail.OnPlayerInvalid += trail.StopTracking;
    trail.OnPlayerDidntMove += () => {
      Server.PrintToChatAll("Player didn't move");
    };
    trail.OnPlayerInvalid += () => {
      // If the player left mid-run, we need to pick the nearest player
      // to continue the run
      var end = trail.GetEndSegment()?.GetEnd() ?? start;
      if (end == null) {
        panic("Speedrunner is invalid, and we cannot find the start");
        return;
      }

      var nearest = PlayerUtil.GetAlive()
       .Where(p => p.Pawn.IsValid && p.Pawn.Value != null)
       .Where(p => p.Pawn.Value!.IsValid && p.Pawn.Value.AbsOrigin != null)
       .MinBy(p => p.Pawn.Value!.AbsOrigin!.DistanceSquared(end));

      if (nearest == null) {
        panic("Speedrunner is invalid, and we cannot find a new one");
        return;
      }

      speedrunner = nearest;
      nearest.Teleport(end);
      player.SetColor(Color.DodgerBlue);
      Locale.RunnerReassigned(player).ToAllChat();
      Locale.YouAreRunner(RoundUtil.GetTimeRemaining()).ToChat(player);
      trail.StartTracking(player);
    };
    return trail;
  }

  private CCSPlayerController? getRunner() {
    var runner = PlayerUtil.GetRandomFromTeam(rng.Next(2) == 0 ?
      CsTeam.Terrorist :
      CsTeam.CounterTerrorist);
    runner ??= PlayerUtil.GetAlive().FirstOrDefault();
    return runner;
  }

  private void startRound(int seconds) {
    roundStartTime = null;
    if (!isRoundActive) {
      panic("Round is not active but we are in startRound");
      return;
    }

    var alive = PlayerUtil.GetAlive().ToArray();
    playersAliveAtStart = PlayerUtil.GetAlive().Count();
    Locale.BeginRound(++round, getEliminations(playersAliveAtStart), seconds)
     .ToAllChat();

    RoundUtil.SetTimeRemaining(seconds + CvFreezeTime.Value);

    foreach (var player in alive) {
      var pawn = player.PlayerPawn.Value;
      if (pawn == null) continue;
      pawn.Teleport(start);
      player.Freeze();
      player.RemoveWeapons();
    }

    resetTrails();
    finishTimestamps.Clear();

    Plugin.AddTimer(CvFreezeTime.Value, () => {
      if (!isRoundActive) return;
      foreach (var player in PlayerUtil.GetAlive()) player.UnFreeze();
      roundStartTime = Server.CurrentTime;
    }, TimerFlags.STOP_ON_MAPCHANGE);

    roundEndTimer = Plugin.AddTimer(seconds + CvFreezeTime.Value, endRound,
      TimerFlags.STOP_ON_MAPCHANGE);
  }

  private void checkFinishers() {
    if (target == null || roundStartTime == null) return;
    if (finishCheckTimer == null) return;
    if (!isRoundActive) {
      panic("Round is not active but we are in checkFinishers");
      return;
    }

    targetCircle?.SetRadius(getRequiredDistance() / 2);
    targetCircle?.Update();
    var required = MathF.Pow(getRequiredDistance(), 2);
    foreach (var player in PlayerUtil.GetAlive()) {
      if (finishTimestamps.ContainsKey(player.Slot)) continue;
      var pos = player.Pawn.Value?.AbsOrigin;
      if (pos == null) continue;
      var hdist = pos.HorizontalDistanceSquared(target);
      if (hdist >= required) continue;
      var dist = pos.DistanceSquared(target);
      if (dist >= required * 1.25f) continue;
      onFinish(player);
    }
  }

  private void onFinish(CCSPlayerController player) {
    if (roundStartTime == null) {
      generics.Error("roundStartTime is null").ToAllChat();
      return;
    }

    var time = Server.CurrentTime - roundStartTime!.Value;
    if (bestTime == null || time < bestTime) {
      bestTime           = time;
      bestTimePlayerSlot = player.Slot;
      Locale.BestTime(player, time).ToAllChat();
      player.SetColor(Color.FromArgb(255, Color.Gold));
    } else {
      Locale.PlayerTime(player, finishTimestamps.Count + 1, -time).ToAllChat();
    }

    finishTimestamps[player.Slot] = -Server.CurrentTime;
    var eliminations = getEliminations(PlayerUtil.GetAlive().Count());
    activeTrails[player.Slot].StopTracking();

    var taking = playersAliveAtStart - eliminations;

    if (finishTimestamps.Count >= taking) endRound();

    if (!player.IsValid) {
      generics.Error("completer is not valid").ToAllChat();
      return;
    }

    if (bestTimePlayerSlot != null && bestTimePlayerSlot == player.Slot) return;

    var alpha = Math.Max(255 - finishTimestamps.Count * 20, 0);
    player.SetColor(Color.FromArgb(alpha, Color.White));
  }

  private void resetTrails() {
    if (activeTrails.Count != 0 && finishTimestamps.Count != 0) {
      var completers = finishTimestamps.Where(x => x.Value < 0).ToArray();
      if (completers.Length > 0) {
        // Of the players who finished, find the one who finished the fastest
        // since these times are negative timestamps, we want the "largest"
        // value (i.e. least negative)
        var best       = completers.MaxBy(x => x.Value);
        var bestPlayer = best.Key;
        var time       = best.Value;
        if (bestTime == null || time <= bestTime) {
          bestTrail?.Kill();
          activeTrails[bestPlayer].StopTracking();
          // bestTrail = BeamTrail.FromTrail(Plugin, activeTrails[bestPlayer]);
          bestTrail = PulsatingBeamTrail.FromTrail(Plugin,
            activeTrails[bestPlayer]);
        }
      }
    }

    foreach (var trail in activeTrails.Values) trail.Kill();

    activeTrails.Clear();

    foreach (var player in PlayerUtil.GetAlive())
      activeTrails[player.Slot] =
        new ActiveInvisiblePlayerTrail(Plugin, player, 0f, MAX_POINTS);
  }

  // https://www.desmos.com/calculator/e1qwgpmtmz
  private float getRequiredDistance() {
    if (roundStartTime == null) return 0;
    var elapsedSeconds = (float)(Server.CurrentTime - roundStartTime);

    return 10 + elapsedSeconds + MathF.Pow(elapsedSeconds, 3.3f) / 2500;
  }

  private void endRound() {
    roundEndTimer?.Kill();
    if (target == null) {
      panic("Target is null");
      return;
    }

    if (!isRoundActive) {
      panic("Round is not active but we are in endRound");
      return;
    }

    var aliveCount          = PlayerUtil.GetAlive().Count();
    var playersDiedMidRound = playersAliveAtStart - aliveCount;
    var toEliminate         = getEliminations(aliveCount) - playersDiedMidRound;

    var ctMade = PlayerUtil.FromTeam(CsTeam.CounterTerrorist).Count() < 4;
    var tMade  = PlayerUtil.FromTeam(CsTeam.Terrorist).Count() < 4;

    foreach (var player in PlayerUtil.GetAlive()) {
      if (player.Team == CsTeam.CounterTerrorist) ctMade = true;
      if (player.Team == CsTeam.Terrorist) tMade         = true;
      if (finishTimestamps.ContainsKey(player.Slot)) continue;

      var dist = player.PlayerPawn.Value?.AbsOrigin?.Distance(target);
      if (dist == null) continue;
      finishTimestamps[player.Slot] = dist.Value;
    }

    if (aliveCount > 1)
      if (ctMade != tMade && round == 1) {
        var random = PlayerUtil.GetRandomFromTeam(tMade ?
          CsTeam.CounterTerrorist :
          CsTeam.Terrorist);

        if (random != null && activeTrails.TryGetValue(random.Slot,
          out var randomTrail)) {
          Locale.ImpossibleLocation(
            ctMade ? CsTeam.Terrorist : CsTeam.CounterTerrorist, random);

          bestTrail?.Kill();
          randomTrail.StopTracking();
          bestTrail = PulsatingBeamTrail.FromTrail(Plugin, randomTrail);
          target    = bestTrail!.GetEndSegment()!.GetEnd();
        }

        toEliminate = 2;
        round--;
      }

    announceTimes();
    var slowTimes     = SlowestTimes(finishTimestamps);
    var keyValuePairs = slowTimes.ToList();

    if (aliveCount <= 2) {
      // Announce winners, end the round, etc.
      // Maybe tp the loser to the winner and let the winner kill them

      if (keyValuePairs.Count == 0) {
        generics.Error("No slowest times found").ToAllChat();
        return;
      }

      var winner = Utilities.GetPlayerFromSlot(keyValuePairs.Last().Key);

      if (winner == null || !winner.IsValid) {
        panic("Winner is null");
        return;
      }

      targetCircle?.Remove();
      targetCircle = null;

      var loser = PlayerUtil.GetAlive()
       .FirstOrDefault(p => p.IsValid && p.Slot != winner.Slot);

      Locale.PlayerWon(winner).ToAllChat();
      if (loser == null || !loser.IsValid) {
        RoundUtil.SetTimeRemaining(10);
        Server.ExecuteCommand("mp_ignore_round_win_conditions 0");
        return;
      }

      loser.SetColor(Color.FromArgb(254, Color.White));
      loser.Teleport(winner);
      EnableDamage(loser);

      winner.GiveNamedItem("weapon_knife");
      winner.GiveNamedItem("weapon_negev");

      RoundUtil.SetTimeRemaining(30);
      Server.ExecuteCommand("mp_ignore_round_win_conditions 0");
      return;
    }

    // var fastTime = MathF.Abs(fastTimestamp) - roundStartTime!;
    var roundTimeWas = Math.Ceiling(Server.CurrentTime - roundStartTime!.Value);
    var nextRoundTime = (int)Math.Ceiling((bestTime ?? 20) + 10 - round * 2);

    if (toEliminate <= 0) {
      Locale.NoneEliminated.ToAllChat();
      Plugin.AddTimer(3f, () => { startRound(nextRoundTime); },
        TimerFlags.STOP_ON_MAPCHANGE);
      return;
    }

    nextRoundTime = (int)Math.Clamp(nextRoundTime, 5, roundTimeWas);
    var slowestEnumerator = SlowestTimes(finishTimestamps).GetEnumerator();

    if (ctMade != tMade && round == 0) {
      bool killedCt = false, killedT = false;
      while (slowestEnumerator.MoveNext()) {
        var (slot, _) = slowestEnumerator.Current;
        var player = Utilities.GetPlayerFromSlot(slot);
        if (player == null || !player.IsValid) continue;
        switch (player.Team) {
          case CsTeam.CounterTerrorist when !killedCt:
            killedCt = true;
            eliminatePlayer(player);
            toEliminate--;
            break;
          case CsTeam.Terrorist when !killedT:
            killedT = true;
            eliminatePlayer(player);
            toEliminate--;
            break;
        }

        if (killedCt && killedT) break;
      }
    }

    for (var i = 0; i < toEliminate; i++) {
      if (!slowestEnumerator.MoveNext()) break;
      var (slot, _) = slowestEnumerator.Current;
      var player = Utilities.GetPlayerFromSlot(slot);
      if (player == null || !player.IsValid) continue;
      EnableDamage(player);
      player.CommitSuicide(false, true);
      Locale.PlayerEliminated(player).ToAllChat();
    }

    slowestEnumerator.Dispose();

    Plugin.AddTimer(3f, () => { startRound(nextRoundTime); },
      TimerFlags.STOP_ON_MAPCHANGE);
  }

  private void eliminatePlayer(CCSPlayerController player) {
    EnableDamage(player);
    player.CommitSuicide(false, true);
    Locale.PlayerEliminated(player).ToAllChat();
  }

  private void panic(string reason) {
    generics.Error($"PANIC: {reason}").ToAllChat();
    Server.ExecuteCommand("mp_ignore_round_win_conditions 0");
    RoundUtil.SetTimeRemaining(1);
  }

  private int getEliminations(int players) {
    return players switch {
      <= 3  => 1,
      <= 4  => 2,
      <= 8  => 3,
      <= 12 => 3,
      <= 20 => 6,
      <= 35 => 8,
      <= 40 => 10,
      <= 64 => 12,
      _     => players / 5
    };
  }

  private void announceTimes() {
    var times = SlowestTimes(finishTimestamps).ToArray();
    for (var i = 0; i < times.Length; i++) {
      var (slot, time) = times[i];
      var player = Utilities.GetPlayerFromSlot(slot);
      if (player == null) continue;
      if (time > 0)
        Locale.PlayerTime(player, times.Length - i, time).ToChat(player);
    }
  }

  public static IEnumerable<KeyValuePair<int, float>>
    SlowestTimes(IDictionary<int, float> timeDistances) {
    return timeDistances.OrderByDescending(entry => entry.Value >= 0)
     .ThenByDescending(entry => entry.Value >= 0 ? entry.Value : float.MinValue)
     .ThenBy(entry => entry.Value < 0 ? entry.Value : float.MaxValue);
  }

  override protected HookResult
    OnEnd(EventRoundEnd @event, GameEventInfo info) {
    var result = base.OnEnd(@event, info);

    finishCheckTimer?.Kill();
    finishCheckTimer = null;
    bestTrail?.Kill();
    roundEndTimer?.Kill();

    foreach (var trail in activeTrails.Values) trail.Kill();

    activeTrails.Clear();

    return result;
  }

  public class SpeedrunSettings : SpecialDaySettings {
    public SpeedrunSettings() {
      CtTeleport = TeleportType.RANDOM_STACKED;
      TTeleport = TeleportType.RANDOM_STACKED;
      StripToKnife = true;
      ConVarValues["mp_ignore_round_win_conditions"] = true;
      WithFriendlyFire();
    }

    public override Func<int> RoundTime
      => () => CvInitialSpeedrunTime.Value + CvFirstRoundFreeze.Value;

    public override ISet<string> AllowedWeapons(CCSPlayerController player) {
      // Return empty set to allow no weapons
      return new HashSet<string>();
    }

    public override float FreezeTime(CCSPlayerController player) {
      return CvFirstRoundFreeze.Value;
    }
  }
}