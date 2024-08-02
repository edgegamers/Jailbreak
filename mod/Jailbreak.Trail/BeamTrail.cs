using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Trail;

namespace Jailbreak.Trail;

public class BeamTrail(BasePlugin plugin, float lifetime = 20,
  int maxPoints = 100) : AbstractTrail<BeamTrailSegment>(lifetime, maxPoints) {
  public static BeamTrail? FromTrail<T>(BasePlugin plugin,
    AbstractTrail<T> trail) where T : ITrailSegment {
    var beamTrail = new BeamTrail(plugin, trail.Lifetime, trail.MaxPoints);
    foreach (var segment in trail)
      beamTrail.Segments.Add(
        beamTrail.CreateSegment(segment.GetStart(), segment.GetEnd()));

    return beamTrail;
  }

  public override BeamTrailSegment CreateSegment(Vector start, Vector end) {
    var beam = new BeamLine(plugin, start, end);
    return new BeamTrailSegment(beam);
  }
}