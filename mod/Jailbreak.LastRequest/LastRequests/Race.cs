using System.Drawing;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.LastRequest.LastRequests;

public class Race(BasePlugin plugin, ILastRequestManager manager,
  CCSPlayerController prisoner, CCSPlayerController guard,
  IRaceLRMessages messages)
  : TeleportingRequest(plugin, manager, prisoner, guard) {
  private DateTime raceStart;
  private Timer? raceTimer;
  private BeamCircle? start, end;
  private Vector? startLocation, endLocation;
  public override LRType type => LRType.Race;

  public override void Setup() {
    base.Setup();

    prisoner.RemoveWeapons();

    guard.RemoveWeapons();
    guard.GiveNamedItem("weapon_knife");

    plugin.AddTimer(3, () => {
      if (state != LRState.Pending) return;
      prisoner.GiveNamedItem("weapon_knife");
    });

    messages.END_RACE_INSTRUCTION.ToPlayerChat(prisoner);

    messages.RACE_STARTING_MESSAGE(prisoner).ToPlayerChat(guard);

    startLocation = prisoner.Pawn?.Value?.AbsOrigin?.Clone();

    if (startLocation == null) return;
    start = new BeamCircle(plugin, startLocation, 20, 16);
    start.SetColor(Color.Aqua);
    start.Draw();
  }

  // Called when the prisoner types !endrace
  public override void Execute() {
    state = LRState.Active;

    endLocation = prisoner.Pawn?.Value?.AbsOrigin?.Clone();

    if (endLocation == null) return;
    end = new BeamCircle(plugin, endLocation, 10, 32);
    end.SetColor(Color.Red);
    end.Draw();

    prisoner.Pawn?.Value?.Teleport(startLocation);
    guard.Pawn.Value?.Teleport(startLocation);

    guard.Freeze();
    prisoner.Freeze();

    plugin.AddTimer(1, () => {
      guard.UnFreeze();
      prisoner.UnFreeze();
    });

    raceStart = DateTime.Now;

    raceTimer = plugin.AddTimer(0.1f, tick, TimerFlags.REPEAT);
  }

  private void tick() {
    if (prisoner.AbsOrigin == null || guard.AbsOrigin == null) return;
    var requiredDistance       = getRequiredDistance();
    var requiredDistanceSqured = MathF.Pow(requiredDistance, 2);

    end?.SetRadius(requiredDistance / 2);
    end?.Update();

    var guardDist = guard.Pawn.Value.AbsOrigin.Clone()
     .DistanceSquared(endLocation);

    if (guardDist < requiredDistanceSqured) {
      manager.EndLastRequest(this, LRResult.GuardWin);
      return;
    }

    var prisonerDist = prisoner.Pawn.Value.AbsOrigin.Clone()
     .DistanceSquared(endLocation);
    if (prisonerDist < requiredDistanceSqured)
      manager.EndLastRequest(this, LRResult.PrisonerWin);
  }

  // https://www.desmos.com/calculator/e1qwgpmtmz
  private float getRequiredDistance() {
    var elapsedSeconds = (DateTime.Now - raceStart).TotalSeconds;

    return (float)(10 + elapsedSeconds + Math.Pow(elapsedSeconds, 2.9) / 5000);
  }

  public override void OnEnd(LRResult result) {
    state = LRState.Completed;
    switch (result) {
      case LRResult.GuardWin:
        prisoner.Pawn.Value?.CommitSuicide(false, true);
        break;
      case LRResult.PrisonerWin:
        guard.Pawn.Value?.CommitSuicide(false, true);
        break;
    }

    raceTimer?.Kill();
    start?.Remove();
    end?.Remove();
  }
}