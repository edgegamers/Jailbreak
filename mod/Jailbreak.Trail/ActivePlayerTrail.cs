﻿using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Trail;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.Trail;

public abstract class ActivePlayerTrail<T> : AbstractTrail<T>
  where T : ITrailSegment {
  public CCSPlayerController Player { get; protected set; }
  protected readonly Timer Timer;

  public ActivePlayerTrail(BasePlugin plugin, CCSPlayerController player,
    float lifetime = 20, int maxPoints = 100, float updateRate = 0.5f) : base(
    plugin, lifetime, maxPoints, updateRate) {
    Player = player;
    Timer  = plugin.AddTimer(updateRate, Tick);
  }

  virtual protected void Tick() {
    if (!Player.IsValid) Kill();
    var pos = Player.PlayerPawn.Value?.AbsOrigin;
    if (pos == null) return;
    AddTrailPoint(pos);
  }

  public void StopTracking() { Timer.Kill(); }

  public virtual void Kill() {
    foreach (var segment in Segments) { segment.Remove(); }

    StopTracking();
  }
}