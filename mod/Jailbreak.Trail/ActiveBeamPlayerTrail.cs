using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Mod.Draw;

namespace Jailbreak.Trail;

public class ActiveBeamPlayerTrail(BasePlugin plugin, CCSPlayerController player,
  float lifetime = 20, int maxPoints = 100, float updateRate = 0.5f)
  : ActivePlayerTrail<BeamTrailSegment>(plugin, player, lifetime, maxPoints,
    updateRate) {
  public override BeamTrailSegment CreateSegment(Vector start, Vector end) {
    var beam = new BeamLine(Plugin, start, end);
    return new BeamTrailSegment(beam);
  }
}