using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Trail;

public class ActiveInvisiblePlayerTrail(BasePlugin plugin,
  CCSPlayerController player, float lifetime = 20, int maxPoints = 100,
  float updateRate = 0.5f)
  : ActivePlayerTrail<VectorTrailSegment>(plugin, player, lifetime, maxPoints,
    updateRate) {
  public override VectorTrailSegment CreateSegment(Vector start, Vector end) {
    return new VectorTrailSegment(start, end);
  }
}