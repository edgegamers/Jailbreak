using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Mod.Zones;

namespace Jailbreak.Zones;

public class ZoneMovementRestrictor : MovementRestrictor {
  private readonly IZone zone;

  public ZoneMovementRestrictor(BasePlugin plugin, CCSPlayerController player,
    IZone zone, float radiusSquared = 250000, Action? onTeleport = null) : base(
    plugin, player, radiusSquared, onTeleport) {
    this.zone = zone;
  }

  public override float DistanceFrom(Vector vec) {
    return zone.GetMinDistance(vec);
  }

  public override Vector GetCenter() { return zone.GetCenterPoint(); }
}