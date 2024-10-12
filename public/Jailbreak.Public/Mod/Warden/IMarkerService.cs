using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Extensions;

namespace Jailbreak.Public.Mod.Warden;

public interface IMarkerService {
  Vector? MarkerPosition { get; }
  float radius { get; }

  public bool InMarker(Vector pos) {
    if (MarkerPosition == null) return false;
    var widenedRadius = radius + 32;
    return MarkerPosition.DistanceSquared(pos) <= widenedRadius * widenedRadius;
  }

  public bool InMarker(CCSPlayerController player) {
    if (MarkerPosition == null) return false;
    var pos = player.PlayerPawn.Value?.AbsOrigin;
    return pos != null && InMarker(pos);
  }
}