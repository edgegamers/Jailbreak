using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
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
using Jailbreak.Validator;
using Microsoft.Extensions.DependencyInjection;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.SpecialDay.SpecialDays;

public class SpeedrunDay(BasePlugin plugin, IServiceProvider provider)
  : AbstractSpecialDay(plugin, provider), ISpecialDayMessageProvider {
  private const int MAX_POINTS = 500;

  public static readonly FakeConVar<int> CvInitialSpeedrunTime =
    new("css_jb_speedrun_initial_time",
      "Duration in seconds to grant the speedrunner", 20);

  public static readonly FakeConVar<int> CvFirstRoundFreeze =
    new("css_jb_speedrun_first_round_freeze",
      "Duration in seconds to give players time to read the rules of speedrun",
      6);

  public static readonly FakeConVar<int> CvFreezeTime =
    new("css_jb_speedrun_freeze_time",
      "Duration in seconds to freeze players before the speedrun starts", 2);

  public static readonly FakeConVar<int> CvWinTimeBase =
    new("css_jb_speedrun_win_time_base",
      "Base duration in seconds to give the winner to kill other competitors",
      25);

  public static readonly FakeConVar<int> CvWinTimeBonus =
    new("css_jb_speedrun_win_time_bonus",
      "Bonus duration in seconds to give the winner for every competitor", 5);

  public static readonly FakeConVar<int> CvWinTimeMax =
    new("css_jb_speedrun_win_max",
      "Max time to give the winner regardless of bonus", 60);

  public static readonly FakeConVar<string> CvWinWeapon = new(
    "css_jb_speedrun_win_weapons",
    "Weapon(s) to give to the winner to kill other competitors",
    "weapon_knife,weapon_negev",
    customValidators: new WeaponValidator(allowMultiple: true));

  public static readonly FakeConVar<string> CvLosersWeapon = new(
    "css_jb_speedrun_loser_weapons",
    "Weapon(s) to give to the losers to use against the winner", "",
    customValidators: new WeaponValidator(allowMultiple: true));

  public static readonly FakeConVar<bool> CvWinnerDamageable = new(
    "css_jb_speedrun_winner_damageable", "Whether the winner can be damaged");

  public static readonly FakeConVar<bool> CvLoserDamageable = new(
    "css_jb_speedrun_loser_damageable", "Whether the losers can be damaged",
    true);

  public static readonly FakeConVar<int> CvTeleportType =
    new("css_jb_speedrun_teleport_type",
      "0 = Dont teleport at end, 1 = Teleport losers to winner, 2 = Teleport winner to loser(s)",
      2);

  public static readonly FakeConVar<int> CvMaxPlayersToFinish = new(
    "css_jb_speedrun_finish_at",
    "Number of players required to declare a winner", 2,
    customValidators: new RangeValidator<int>(2, 10));

  public static readonly FakeConVar<int> CvOffset1 = new(
    "css_lines", "Offset 1", 8);

  public static readonly FakeConVar<int> CvOffset2 = new(
    "css_lines_below", "Offset 2", 4);

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
  // private Timer? finishCheckTimer;

  private IGenericCmdLocale generics = null!;
  private int round, playersAliveAtStart;
  private Timer? roundEndTimer;

  private float? roundStartTime;
  private CCSPlayerController? speedrunner;
  private Vector? start;
  private Vector? target;
  private BeamCircle? targetCircle;
  private ISpeedDayLocale msg => (ISpeedDayLocale)Locale;

  private bool isRoundActive
    => Provider.GetRequiredService<ISpecialDayManager>().CurrentSD == this;

  public override SDType Type => SDType.SPEEDRUN;

  public override SpecialDaySettings Settings => new SpeedrunSettings();

  public ISDInstanceLocale Locale => new SpeedrunDayLocale();

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
      if (!speedrunner.IsValid || speedrunner.Connected
        != PlayerConnectedState.PlayerConnected)
        speedrunner = getRunner();
      if (speedrunner == null) {
        panic("Speedrunner is invalid, and we cannot find a new one");
        return;
      }

      msg.RunnerAssigned(speedrunner).ToAllChat();
      speedrunner.SetColor(Color.DodgerBlue);
      msg.YouAreRunner(CvInitialSpeedrunTime.Value).ToChat(speedrunner);
    };
    Timers[CvFirstRoundFreeze.Value] += () => {
      if (!speedrunner.IsValid || speedrunner.Connected
        != PlayerConnectedState.PlayerConnected) {
        speedrunner = getRunner();
        if (speedrunner == null) {
          panic(
            "Original speedrunner is invalid, and we cannot find a new one");
          return;
        }

        speedrunner.SetColor(Color.DodgerBlue);
        msg.RunnerReassigned(speedrunner).ToAllChat();
        msg.YouAreRunner(CvInitialSpeedrunTime.Value).ToChat(speedrunner);
      }

      start = speedrunner.PlayerPawn.Value!.AbsOrigin!.Clone();
      speedrunner.UnFreeze();
      bestTrail = createFirstTrail(speedrunner);
    };

    if (CvInitialSpeedrunTime.Value > 30)
      Timers[CvInitialSpeedrunTime.Value + CvFirstRoundFreeze.Value - 30] += ()
        => {
        if (target != null) return;
        if (!speedrunner.IsValid || speedrunner.Connected
          != PlayerConnectedState.PlayerConnected)
          speedrunner = getRunner();
        if (speedrunner == null) {
          panic(
            "Original speedrunner is invalid, and we cannot find a new one");
          return;
        }

        msg.RuntimeLeft(RoundUtil.GetTimeRemaining()).ToChat(speedrunner);
      };

    Timers[CvInitialSpeedrunTime.Value + CvFirstRoundFreeze.Value - 10] += ()
      => {
      if (target != null) return;
      msg.RuntimeLeft(RoundUtil.GetTimeRemaining()).ToChat(speedrunner);
    };
    Timers[CvInitialSpeedrunTime.Value + CvFirstRoundFreeze.Value] += Execute;

    base.Setup();
  }

  public override void Execute() {
    if (target != null) return; // We started the round already
    if (speedrunner == null) {
      panic("Execute: Speedrunner is null");
      return;
    }

    target = speedrunner.Pawn.Value?.AbsOrigin;
    if (target == null) {
      panic("Execute: Could not get AbsOrigin of speedrunner");
      return;
    }

    target       = target.Clone();
    targetCircle = new BeamCircle(Plugin, target!, 10, 16);
    targetCircle.SetColor(Color.Green);
    targetCircle.Draw();

    if (bestTrail is null) {
      generics.Error("Execute: bestTrail is null").ToAllChat();
      return;
    }

    if (bestTrail is ActivePlayerTrail<BeamTrailSegment> active)
      active.StopTracking();

    var timeSpent = bestTrail.GetEndSegment()!.GetSpawnTime()
      - bestTrail.GetStartSegment()!.GetSpawnTime();

    bestTime = timeSpent;

    var minTime = CvInitialSpeedrunTime.Value * 0.5;

    startRound((int)Math.Ceiling(Math.Max(timeSpent * 1.1, minTime)));

    Plugin.RegisterListener<Listeners.OnTick>(checkFinishers);
  }

  private ActivePlayerTrail<BeamTrailSegment> createFirstTrail(
    CCSPlayerController player) {
    var trail = new ActivePulsatingBeamPlayerTrail(Plugin, player, 0f,
      MAX_POINTS, 0.15f);
    trail.OnPlayerDidntMove += () => {
      if (trail.Player == null) {
        panic("OnPlayerDidntMove: Player is null");
        return;
      }

      var tps              = 1 / trail.UpdateRate;
      var didntMoveSeconds = (int)Math.Ceiling(trail.DidntMoveTicks / tps);
      var thresholdTicks   = (int)Math.Ceiling(tps * 3);
      if (trail.DidntMoveTicks < thresholdTicks) return;
      if (trail.DidntMoveTicks == thresholdTicks)
        msg.StayStillToSpeedup.ToChat(trail.Player);
      if (didntMoveSeconds % 3 == 0) RoundUtil.AddTimeRemaining(-1);
      if (RoundUtil.GetTimeRemaining() <= 0) Execute();
    };
    trail.OnPlayerInvalid -= trail.Kill;
    trail.OnPlayerInvalid += trail.StopTracking;
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
      nearest.Pawn.Value?.Teleport(end);
      nearest.SetColor(Color.DodgerBlue);
      msg.RunnerReassigned(nearest).ToAllChat();
      msg.YouAreRunner(RoundUtil.GetTimeRemaining()).ToChat(nearest);
      trail.StartTracking(nearest);
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
    msg.BeginRound(++round, getEliminations(playersAliveAtStart), seconds)
     .ToAllChat();

    RoundUtil.SetTimeRemaining(seconds + CvFreezeTime.Value);

    foreach (var player in alive) {
      var pawn = player.PlayerPawn.Value;
      if (pawn == null) continue;
      pawn.Teleport(start, velocity: Vector.Zero);
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
    if (!isRoundActive) {
      panic("Round is not active but we are in checkFinishers");
      return;
    }

    targetCircle?.SetRadius(getRequiredDistance() / 2);
    targetCircle?.Update();
    var required = MathF.Pow(getRequiredDistance(), 2);

    var distances = new Dictionary<int, float>(finishTimestamps);
    foreach (var player in PlayerUtil.GetAlive()) {
      if (finishTimestamps.ContainsKey(player.Slot)) continue;
      var pos = player.Pawn.Value?.AbsOrigin;
      if (pos == null) continue;
      var dist = pos.DistanceSquared(target);
      distances[player.Slot] = dist;
      if (dist >= required * 1.25f) continue;
      var hdist = pos.HorizontalDistanceSquared(target);
      if (hdist >= required) continue;
      onFinish(player);
    }

    sendDistances(distances);
  }

  private void sendDistances(IDictionary<int, float> distances) {
    if (target == null) {
      panic("sendDistances: Target is null");
      return;
    }

    ServerExtensions.GetGameRules().GameRestart =
      ServerExtensions.GetGameRules().RestartRoundTime < Server.CurrentTime;

    const int TOTAL_LINES = 8; // Total number of lines to display

    // Sort distances by score (ascending order)
    var sortedDistances = distances.OrderBy(d => d.Value).ToList();

    // Total number of players
    var playerCount = sortedDistances.Count;

    // Dictionary to store scoreboards for each player
    var scoreboards = new Dictionary<int, LinkedList<string>>();

    for (var i = 0; i < playerCount; i++) {
      var linkedList = new LinkedList<string>();

      // Determine the range to display
      var startIndex = Math.Max(0, i - TOTAL_LINES / 2);
      var endIndex   = Math.Min(playerCount - 1, i + TOTAL_LINES / 2);

      // Adjust the range to always show TOTAL_LINES players
      if (endIndex - startIndex + 1 < TOTAL_LINES) {
        if (startIndex == 0)
          endIndex = Math.Min(TOTAL_LINES - 1, playerCount - 1);
        else if (endIndex == playerCount - 1)
          startIndex = Math.Max(0, playerCount - TOTAL_LINES);
      }

      for (var j = startIndex; j <= endIndex; j++) {
        var (slot, dist) = sortedDistances[j];
        var player = Utilities.GetPlayerFromSlot(slot);
        if (player == null || !player.IsValid) continue;

        var line         = generateHTMLLine(player, j + 1, dist);
        if (j != i) line = $"<span class=\"fontSize-sm\">{line}</span>";
        linkedList.AddLast(line);
      }

      scoreboards[sortedDistances[i].Key] = linkedList;
    }

    // Print scoreboards for each player
    foreach (var (slot, _) in sortedDistances) {
      var player = Utilities.GetPlayerFromSlot(slot);
      if (player == null || !player.IsValid) continue;

      var scoreboard = scoreboards[slot];
      var message    = string.Join("<br>", scoreboard);
      player.PrintToCenterHtml(message);
    }

    // Handle disconnected players
    var winningScoreboard = scoreboards.First().Value;
    var winningMessage    = string.Join("<br>", winningScoreboard);
    foreach (var player in Utilities.GetPlayers()
     .Where(p => p is { PawnIsAlive: false, IsBot: false }))
      player.PrintToCenterHtml(winningMessage);
  }

  private string generateHTMLLine(CCSPlayerController player, int position,
    float distance) {
    string color;
    var    eliminations = getEliminations(playersAliveAtStart);
    var    suffix       = "";

    var isSafe     = position < eliminations && distance < 0;
    var isInDanger = position > playersAliveAtStart - eliminations;

    var text = $"{position} {player.PlayerName}";

    if (isSafe) {
      color  = "00FF00";
      suffix = "<font color=\"#00FF00\"> | S</font>";
    } else if (!isInDanger) {
      var percentDanger = (position - 1 - finishTimestamps.Count)
        / (float)eliminations;
      // Gradient from green to yellow
      percentDanger = Math.Clamp(percentDanger, 0, 1);
      var green = 255;
      var red   = (int)(255 * percentDanger);
      color = $"{red:X2}{green:X2}00";
    } else {
      var precentLosing = (position - playersAliveAtStart + eliminations)
        / (float)eliminations;
      // Gradient from orange to red, with red being the most losing
      precentLosing = Math.Clamp(precentLosing, 0, 1);
      var red   = 255;
      var green = 255 - (int)(255 * precentLosing);
      color  = $"{red:X2}{green:X2}00";
      suffix = "<font color=\"#FF0000\"> | E</font>";
    }

    if (distance < 0) {
      var time = roundStartTime == null ?
        0 :
        MathF.Abs(distance) - roundStartTime.Value;
      text += $" - {time:F4}";
    } else { text += $" - {distance:N0}"; }

    return $"<font color=\"#{color}\">{text}</font>{suffix}";
  }

  private void onFinish(CCSPlayerController player) {
    if (roundStartTime == null) {
      panic("onFinish: roundStartTime is null");
      return;
    }

    var time = Server.CurrentTime - roundStartTime!.Value;
    if (bestTime == null || time < bestTime) {
      bestTime           = time;
      bestTimePlayerSlot = player.Slot;
      msg.BestTime(player, time).ToAllChat();
      player.SetColor(Color.FromArgb(255, Color.Gold));
    } else {
      msg.PlayerTime(player, finishTimestamps.Count + 1, -time).ToAllChat();
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
      panic("endRound: Target is null");
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
          msg.ImpossibleLocation(
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

    if (aliveCount <= CvMaxPlayersToFinish.Value) {
      // Announce winners, end the round, etc.
      // Maybe tp the loser to the winner and let the winner kill them

      if (keyValuePairs.Count == 0) {
        generics.Error("No slowest times found").ToAllChat();
        return;
      }

      var winner = Utilities.GetPlayerFromSlot(keyValuePairs.Last().Key);

      if (winner == null || !winner.IsValid) {
        panic("endRound: Winner is null");
        return;
      }

      targetCircle?.Remove();
      targetCircle = null;

      var losers = PlayerUtil.GetAlive()
       .Where(p => p.Slot != winner.Slot)
       .ToList();

      var timeToSet = CvWinTimeBase.Value + CvWinTimeBonus.Value * losers.Count;

      msg.PlayerWon(winner).ToAllChat();

      foreach (var loser in losers) {
        loser.SetColor(Color.FromArgb(254, Color.White));
        if (CvTeleportType.Value == 1)
          loser.Teleport(winner);
        else if (CvTeleportType.Value == 2) winner.Teleport(loser);
        if (CvLoserDamageable.Value) EnableDamage(loser);
      }

      if (CvWinnerDamageable.Value) EnableDamage(winner);

      foreach (var weapon in CvLosersWeapon.Value.Split(','))
        foreach (var loser in losers)
          loser.GiveNamedItem(weapon);

      foreach (var weapon in CvWinWeapon.Value.Split(','))
        winner.GiveNamedItem(weapon);

      RoundUtil.SetTimeRemaining(Math.Min(timeToSet, CvWinTimeMax.Value));
      Server.ExecuteCommand("mp_ignore_round_win_conditions 0");
      return;
    }

    // var fastTime = MathF.Abs(fastTimestamp) - roundStartTime!;
    var roundTimeWas = Math.Ceiling(Server.CurrentTime - roundStartTime!.Value);
    var nextRoundTime = (int)Math.Ceiling((bestTime ?? 20) + 10 - round * 1.5);

    if (toEliminate <= 0) {
      msg.NoneEliminated.ToAllChat();
      Plugin.AddTimer(3f, () => { startRound(nextRoundTime); },
        TimerFlags.STOP_ON_MAPCHANGE);
      return;
    }

    nextRoundTime = (int)Math.Min(roundTimeWas, Math.Max(nextRoundTime, 5));
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
      msg.PlayerEliminated(player).ToAllChat();
    }

    slowestEnumerator.Dispose();

    Plugin.AddTimer(3f, () => { startRound(nextRoundTime); },
      TimerFlags.STOP_ON_MAPCHANGE);
  }

  private void eliminatePlayer(CCSPlayerController player) {
    EnableDamage(player);
    player.CommitSuicide(false, true);
    msg.PlayerEliminated(player).ToAllChat();
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
      <= 6  => 2,
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
        msg.PlayerTime(player, times.Length - i, time).ToChat(player);
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

    Plugin.RemoveListener<Listeners.OnTick>(checkFinishers);
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
      RestrictWeapons = true;
      ConVarValues["mp_ignore_round_win_conditions"] = true;
      WithAutoBhop();
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