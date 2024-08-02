using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Mod.Draw;

namespace Jailbreak.Trail;

public class ActivePulsatingBeamPlayerTrail(BasePlugin plugin,
  CCSPlayerController player, float lifetime = 20, int maxPoints = 100,
  float updateRate = 0.5f)
  : ActivePlayerTrail<BeamTrailSegment>(plugin, player, lifetime, maxPoints,
    updateRate) {
  private PulsatingBeamTrail trail = new PulsatingBeamTrail(plugin, lifetime,
    maxPoints, updateRate);

  public override void Kill() { trail.Kill(); }

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
}