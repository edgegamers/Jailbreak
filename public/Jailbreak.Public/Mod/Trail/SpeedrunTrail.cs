using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using CS2DrawShared;
using Jailbreak.Public.Extensions;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.Public.Mod.Trail;

/// <summary>
/// Wraps ITrailHandle with speedrun-specific semantics:
/// - Tracks a player, fires OnPlayerDidntMove and OnPlayerInvalid events
/// - Invisible by default — FadeIn() on finish
/// - Freeze() detaches from pawn and drops a static anchor
/// - Remove() cleans up both the trail and any static anchor
/// </summary>
public sealed class SpeedrunTrail {
  private readonly ITrailHandle handle;
  private readonly BasePlugin plugin;
  private CInfoTarget? frozenAnchor;
  private Timer? timer;

  private const int ALPHA_CP = 3;
  private const float MIN_MOVE_DIST_SQ = 1000f;

  public CCSPlayerController? Player { get; private set; }
  public float UpdateRate { get; private set; }
  public int DidntMoveTicks { get; set; }

  public bool IsRunning => handle.IsRunning;
  public bool IsFrozen => frozenAnchor != null;

  public event Action OnPlayerInvalid = () => { };
  public event Action OnPlayerDidntMove = () => { };

  public SpeedrunTrail(BasePlugin plugin, ITrailHandle handle,
    CCSPlayerController player, float updateRate = 0.5f) {
    this.plugin = plugin;
    this.handle = handle ?? throw new ArgumentNullException(nameof(handle));
    Player      = player;
    UpdateRate  = updateRate;

    // All speedrun trails start invisible — faded in on finish
    SetAlpha(0f);

    timer = plugin.AddTimer(UpdateRate, tick,
      TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
  }

  // ── Tick ──────────────────────────────────────────────────────────────────

  private void tick() {
    if (Player == null) return;

    if (!Player.IsValid
      || Player.Connected != PlayerConnectedState.PlayerConnected) {
      OnPlayerInvalid.Invoke();
      return;
    }

    var pos = Player.PlayerPawn.Value?.AbsOrigin;
    if (pos == null) return;

    // Check if player has moved enough since the last tick
    var pawn = Player.PlayerPawn.Value;
    if (pawn?.AbsOrigin == null) return;

    // We don't track positions ourselves — CS2Draw does that via the trail.
    // We just need to know if the player is moving to fire events.
    var lastPos = pawn.AbsOrigin;
    var distSq  = lastPos.DistanceSquared(pos);

    if (distSq < MIN_MOVE_DIST_SQ) {
      DidntMoveTicks++;
      OnPlayerDidntMove.Invoke();
      return;
    }

    DidntMoveTicks = 0;
  }

  // ── Tracking control ──────────────────────────────────────────────────────

  public void StopTracking() {
    timer?.Kill();
    timer = null;
  }

  public void StartTracking(CCSPlayerController? player = null,
    float? updateRate = null) {
    if (player != null) Player         = player;
    if (updateRate != null) UpdateRate = updateRate.Value;

    // Re-parent trail to new player pawn
    if (Player?.PlayerPawn.Value != null)
      handle.SetParent(Player.PlayerPawn.Value);

    timer?.Kill();
    timer = plugin.AddTimer(UpdateRate, tick,
      TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);

    DidntMoveTicks = 0;
  }

  // ── Lifecycle ─────────────────────────────────────────────────────────────

  /// <summary>
  /// Detach from the moving player pawn and parent to a static anchor
  /// at their current position. Stops emitting — existing particles remain.
  /// </summary>
  public void Freeze(Vector position) {
    if (IsFrozen) return;

    StopTracking();

    frozenAnchor = Utilities.CreateEntityByName<CInfoTarget>("info_target");
    if (frozenAnchor == null) return;

    frozenAnchor.Teleport(position, QAngle.Zero, Vector.Zero);
    frozenAnchor.DispatchSpawn();

    handle.SetParent(frozenAnchor);
    handle.Stop();
  }

  /// <summary>Make the trail fully visible.</summary>
  public void FadeIn() => SetAlpha(1f);

  /// <summary>Make the trail invisible without removing it.</summary>
  public void FadeOut() => SetAlpha(0f);

  /// <summary>Set alpha directly — 0.0 invisible, 1.0 fully opaque.</summary>
  public void SetAlpha(float alpha) => handle.SetCp(ALPHA_CP, alpha, 0f, 0f);

  /// <summary>
  /// Stop everything and remove the static anchor.
  /// Existing particles fade naturally.
  /// </summary>
  public void Remove() {
    StopTracking();
    handle.Stop();
    frozenAnchor?.Remove();
    frozenAnchor = null;
  }
}