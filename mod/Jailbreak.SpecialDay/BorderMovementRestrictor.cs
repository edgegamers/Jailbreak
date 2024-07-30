using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.SpecialDay;

public class BorderMovementRestrictor : MovementRestrictor {
  private readonly IBorder border;

  public BorderMovementRestrictor(BasePlugin plugin, CCSPlayerController player,
    IBorder border, float radiusSquared = 250000,
    Action? onTeleport = null) :
    base(plugin, player, radiusSquared, onTeleport) {
    this.border = border;
  }

  public override float DistanceFrom(Vector vec) {
    return border.GetMinDistance(vec);
  }

  public override Vector GetCenter() { return border.GetCenter(); }
}