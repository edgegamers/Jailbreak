using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Draw;

namespace Jailbreak.Trail;

public class ActivePulsatingBeamPlayerTrail(BasePlugin plugin,
  CCSPlayerController player, float lifetime = 20, int maxPoints = 100,
  float updateRate = 0.5f, float pulseRate = 0.5f, float pulseMin = 0.5f,
  float pulseMax = 2.5f)
  : ActivePlayerTrail<BeamTrailSegment>(plugin, player, lifetime, maxPoints,
    updateRate) {
  private readonly PulsatingBeamTrail trail = new(plugin, lifetime, maxPoints,
    updateRate, pulseRate, pulseMin, pulseMax);

  public override void Kill() {
    base.Kill();
    trail.Kill();
  }

  override protected void Tick() {
    if (!Player.IsValid) Kill();
    var pos = Player.PlayerPawn.Value?.AbsOrigin;
    if (pos == null) return;
    pos = pos.Clone();
    var end  = GetEndSegment();
    var dist = end?.GetStart().DistanceSquared(pos) ?? float.MaxValue;
    if (dist < 100) {
      // Still want to remove old segments
      Cleanup();
      return;
    }

    AddTrailPoint(pos);
  }

  public override void StopTracking() { base.StopTracking(); }

  public override void AddTrailPoint(Vector vector) {
    trail.AddTrailPoint(vector);
  }

  public override IList<BeamTrailSegment> GetTrail(float since, int max = 0) {
    return trail.GetTrail(since, max);
  }

  public override IList<Vector> GetTrailPoints(float since, int max = 0) {
    return trail.GetTrailPoints(since, max);
  }

  public override BeamTrailSegment CreateSegment(Vector start, Vector end) {
    return trail.CreateSegment(start, end);
  }

  public override BeamTrailSegment? GetStartSegment() {
    return trail.GetStartSegment();
  }

  public override BeamTrailSegment? GetEndSegment() {
    return trail.GetEndSegment();
  }

  public override IList<Vector> GetTrailPoints(int max) {
    return trail.GetTrailPoints(max);
  }

  public override BeamTrailSegment? GetNearestSegment(Vector position,
    float since, int max = 0) {
    return trail.GetNearestSegment(position, since, max);
  }
}