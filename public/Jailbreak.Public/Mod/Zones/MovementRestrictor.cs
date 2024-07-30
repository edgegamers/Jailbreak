using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Extensions;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.SpecialDay;

/// <summary>
/// Wrapper class to restrict player movement to a given area / zone
/// </summary>
public abstract class MovementRestrictor {
  private readonly CCSPlayerController player;
  private Vector? lastValid;
  private readonly float radiusSquared;
  private Timer timer;
  private readonly Action? onTeleport;
  private float maxSpeed = 0.6f;

  public MovementRestrictor(BasePlugin plugin, CCSPlayerController player,
    float radiusSquared = 250000f, Action? onTeleport = null) {
    this.player        = player;
    this.radiusSquared = radiusSquared;
    this.onTeleport    = onTeleport;

    timer = plugin.AddTimer(0.1f, tick,
      TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
    plugin.RegisterEventHandler<EventRoundEnd>(OnEnd);
  }

  public MovementRestrictor WithMaxSpeed(float speed) {
    maxSpeed = speed;
    return this;
  }

  public abstract float DistanceFrom(Vector vec);

  /// <summary>
  /// Returns a valid center point to teleport a player to
  /// in case they somehow got outside the zone
  /// </summary>
  /// <returns></returns>
  public abstract Vector GetCenter();

  private void tick() {
    var pawn = player.PlayerPawn.Value;
    if (!player.IsValid || !player.PawnIsAlive || pawn == null) Kill();

    player.SetSpeed(maxSpeed);
    var dist = DistanceFrom(pawn!.AbsOrigin.Clone());
    if (dist <= radiusSquared) {
      if (dist <= radiusSquared * 0.90 && pawn!.OnGroundLastTick)
        lastValid = player.Pawn.Value!.AbsOrigin!.Clone();
      return;
    }

    onTeleport?.Invoke();
    if (lastValid == null) return;
    player.PlayerPawn.Value?.Teleport(lastValid ?? GetCenter());
  }

  public void Kill() {
    player.SetSpeed(1f);
    timer.Kill();
  }

  private HookResult OnEnd(EventRoundEnd @event, GameEventInfo info) {
    Kill();
    return HookResult.Continue;
  }
}