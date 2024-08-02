using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
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
using Microsoft.VisualBasic.CompilerServices;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.SpecialDay.SpecialDays;

public class SpeedrunDay(BasePlugin plugin, IServiceProvider provider)
  : AbstractSpecialDay(plugin, provider), ISpecialDayMessageProvider {
  private static readonly int FIRST_SPEEDRUNNER_TIME = 60;
  private static readonly int FIRST_ROUND_FREEZE = 8;
  private static readonly int FREEZE_TIME = 2;
  private static readonly int MAX_POINTS = 500;
  private readonly Random rng = new();

  private readonly IDictionary<int, ActivePlayerTrail<VectorTrailSegment>>
    activeTrails = new Dictionary<int, ActivePlayerTrail<VectorTrailSegment>>();

  private AbstractTrail<BeamTrailSegment>? bestTrail;
  private float? bestTime;
  private Timer? finishCheckTimer;

  /// <summary>
  ///   Negative values represent players who finished.
  ///   Positive values represent players who are still alive, and the value
  ///   being the distance they are from the target.
  /// </summary>
  private readonly IDictionary<int, float> finishTimestamps =
    new SortedDictionary<int, float>();

  private IGenericCommandNotifications generics = null!;
  private int round, playersAliveAtStart, bestTimePlayerSlot;
  private Timer? roundEndTimer;

  private float? roundStartTime;
  private Vector? start;
  private Vector? target;
  private BeamCircle? targetCircle;
  public override SDType Type => SDType.SPEEDRUN;

  private SpeedrunDayMessages msg => (SpeedrunDayMessages)Messages;

  public override SpecialDaySettings Settings => new SpeedrunSettings();
  public ISpecialDayInstanceMessages Messages => new SpeedrunDayMessages();

  public override void Setup() {
    generics = provider.GetRequiredService<IGenericCommandNotifications>();

    foreach (var player in Utilities.GetPlayers().Where(p => !p.PawnIsAlive))
      player.Respawn();

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

    Timers[0.1f] += () => {
      // Needed since players who respawned are given knife later
      foreach (var player in PlayerUtil.GetAlive()) player.RemoveWeapons();
    };
    Timers[FIRST_ROUND_FREEZE - 4] += () => {
      msg.RunnerAssigned(speedrunner).ToAllChat();
      speedrunner.SetColor(Color.DodgerBlue);
      msg.YouAreRunner(FIRST_SPEEDRUNNER_TIME).ToPlayerChat(speedrunner);
    };
    Timers[FIRST_ROUND_FREEZE - 2] += () => {
      start = speedrunner.PlayerPawn.Value!.AbsOrigin!.Clone();
      speedrunner.UnFreeze();
      // bestTrail = new ActiveBeamPlayerTrail(plugin, speedrunner, 0f,
      //   updateRate: 0.15f, maxPoints: MAX_POINTS);
      bestTrail = new ActivePulsatingBeamPlayerTrail(plugin, speedrunner, 0f,
        MAX_POINTS, updateRate: 0.15f);
    };

    Timers[FIRST_SPEEDRUNNER_TIME + FIRST_ROUND_FREEZE - 30] += ()
      => msg.RuntimeLeft(30).ToPlayerChat(speedrunner);
    Timers[FIRST_SPEEDRUNNER_TIME + FIRST_ROUND_FREEZE - 10] += ()
      => msg.RuntimeLeft(10).ToPlayerChat(speedrunner);
    Timers[FIRST_SPEEDRUNNER_TIME + FIRST_ROUND_FREEZE] += () => {
      target       = speedrunner.PlayerPawn.Value?.AbsOrigin!.Clone();
      targetCircle = new BeamCircle(plugin, target!, 10, 16);
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

      var minTime = FIRST_SPEEDRUNNER_TIME * 0.5;

      startRound((int)Math.Ceiling(Math.Max(timeSpent * 1.1, minTime)));

      finishCheckTimer = Plugin.AddTimer(0.03f, checkFinishers,
        TimerFlags.STOP_ON_MAPCHANGE | TimerFlags.REPEAT);
    };

    base.Setup();

    foreach (var player in PlayerUtil.GetAlive()) {
      player.SetColor(Color.FromArgb(65, 255, 255, 255));
      player.RemoveWeapons();
    }
  }

  private void startRound(int seconds) {
    roundStartTime = null;
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
      player.RemoveWeapons();
    }

    resetTrails();
    finishTimestamps.Clear();

    Plugin.AddTimer(FREEZE_TIME, () => {
      foreach (var player in PlayerUtil.GetAlive()) player.UnFreeze();
      roundStartTime = Server.CurrentTime;
    }, TimerFlags.STOP_ON_MAPCHANGE);

    roundEndTimer = Plugin.AddTimer(seconds + FREEZE_TIME, endRound,
      TimerFlags.STOP_ON_MAPCHANGE);
  }

  private void checkFinishers() {
    if (target == null || roundStartTime == null) return;
    targetCircle?.SetRadius(getRequiredDistance() / 2);
    targetCircle?.Update();
    var required = MathF.Pow(getRequiredDistance(), 2);
    foreach (var player in PlayerUtil.GetAlive()) {
      if (finishTimestamps.ContainsKey(player.Slot)) continue;
      var pos = player.PlayerPawn.Value?.AbsOrigin;
      if (pos == null) continue;
      var dist = pos.DistanceSquared(target);
      if (dist >= required) continue;
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
      msg.BestTime(player, time).ToAllChat();
      player.SetColor(Color.FromArgb(255, Color.Gold));
    } else
      msg.PlayerTime(player, finishTimestamps.Count + 1, -time).ToAllChat();

    finishTimestamps[player.Slot] = -Server.CurrentTime;
    var eliminations = getEliminations(PlayerUtil.GetAlive().Count());
    activeTrails[player.Slot].StopTracking();

    var taking = playersAliveAtStart - eliminations;

    if (finishTimestamps.Count >= taking) endRound();

    if (bestTimePlayerSlot == player.Slot) return;
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
          // bestTrail = BeamTrail.FromTrail(plugin, activeTrails[bestPlayer]);
          bestTrail = PulsatingBeamTrail.FromTrail(plugin,
            activeTrails[bestPlayer], pulseRate: 0.05f, pulseMin: 0.5f,
            pulseMax: 1.5f);
        }
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

    return 10 + elapsedSeconds + MathF.Pow(elapsedSeconds, 3.3f) / 2500;
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
    var slowTimes     = SlowestTimes(finishTimestamps);
    var keyValuePairs = slowTimes.ToList();
    var fastTimestamp = keyValuePairs.Where(s => s.Value < 0)
     .Select(s => s.Value)
     .LastOrDefault(-(Server.CurrentTime - FIRST_SPEEDRUNNER_TIME));
    if (aliveCount <= 2) {
      // Announce winners, end the round, etc.
      // Maybe tp the loser to the winner and let the winner kill them

      if (keyValuePairs.Count == 0) {
        generics.Error("No slowest times found").ToAllChat();
        return;
      }

      var winner = Utilities.GetPlayerFromSlot(keyValuePairs.First().Key);

      if (winner == null || !winner.IsValid) {
        generics.Error("Winner is null").ToAllChat();
        return;
      }

      targetCircle?.Remove();
      finishCheckTimer?.Kill();

      var loser = PlayerUtil.GetAlive()
       .FirstOrDefault(p => p.IsValid && p.Slot != winner.Slot);

      msg.PlayerWon(winner).ToAllChat();
      if (loser == null || !loser.IsValid) {
        RoundUtil.SetTimeRemaining(10);
        Server.ExecuteCommand("mp_ignore_round_win_conditions 0");
        return;
      }

      loser.Teleport(winner);
      EnableDamage(loser);

      winner.GiveNamedItem(CsItem.Knife);
      winner.GiveNamedItem(CsItem.Negev);
      RoundUtil.SetTimeRemaining(30);
      Server.ExecuteCommand("mp_ignore_round_win_conditions 0");
      return;
    }

    var fastTime = MathF.Abs(fastTimestamp) - roundStartTime!;
    var roundTimeWas = Math.Ceiling(Server.CurrentTime - roundStartTime!.Value);
    var nextRoundTime = (int)Math.Ceiling(fastTime.Value + 10 - round * 2);

    if (toEliminate <= 0) {
      msg.NoneEliminated.ToAllChat();
      Plugin.AddTimer(3f, () => { startRound(nextRoundTime); },
        TimerFlags.STOP_ON_MAPCHANGE);
      return;
    }

    nextRoundTime = (int)Math.Clamp(nextRoundTime, 5, roundTimeWas);
    var slowestEnumerator = SlowestTimes(finishTimestamps).GetEnumerator();

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


  private int getEliminations(int players) {
    return players switch {
      <= 4  => 1,
      <= 8  => 2,
      <= 12 => 3,
      _     => 4
    };
  }

  private void announceTimes() {
    var times = SlowestTimes(finishTimestamps).ToArray();
    for (var i = 0; i < times.Length; i++) {
      var (slot, time) = times[i];
      var player = Utilities.GetPlayerFromSlot(slot);
      if (player == null) continue;
      if (time > 0) msg.PlayerTime(player, i + 1, time).ToPlayerChat(player);
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

    public override Func<int> RoundTime
      => () => FIRST_SPEEDRUNNER_TIME + FIRST_ROUND_FREEZE;

    public override ISet<string>? AllowedWeapons(CCSPlayerController player) {
      // Return empty set to allow no weapons
      return new HashSet<string>();
    }

    public override float FreezeTime(CCSPlayerController player) {
      return FIRST_ROUND_FREEZE;
    }
  }
}