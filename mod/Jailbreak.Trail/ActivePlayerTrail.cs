using System.Runtime.CompilerServices;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Trail;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.Trail;

public abstract class ActivePlayerTrail<T> : AbstractTrail<T>
  where T : ITrailSegment {
  protected readonly BasePlugin Plugin;
  protected Timer? Timer;

  public ActivePlayerTrail(BasePlugin plugin, CCSPlayerController player,
    float lifetime = 20, int maxPoints = 100, float updateRate = 0.5f) : base(
    lifetime, maxPoints) {
    Plugin     = plugin;
    Player     = player;
    UpdateRate = updateRate;
    Timer = plugin.AddTimer(UpdateRate, Tick,
      TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
    OnPlayerInvalid += StopTracking;
  }

  public float UpdateRate { get; protected set; }
  public int DidntMoveTicks { get; protected set; }

  public CCSPlayerController? Player { get; protected set; }
  public event Action OnPlayerInvalid = () => { };
  public event Action OnPlayerDidntMove = () => { };

  virtual protected void Tick() {
    if (Player == null) return;
    if (!Player.IsValid) {
      OnPlayerInvalid.Invoke();
      Player = null;
      return;
    }

    var pos = Player.PlayerPawn.Value?.AbsOrigin;
    if (pos == null) return;
    pos = pos.Clone();
    var end  = GetEndSegment();
    var dist = end?.GetStart().DistanceSquared(pos) ?? float.MaxValue;
    if (dist < 1000) {
      // Still want to remove old segments
      Cleanup();
      DidntMoveTicks++;
      OnPlayerDidntMove.Invoke();
      return;
    }

    DidntMoveTicks = 0;
    AddTrailPoint(pos);
  }

  public virtual void StopTracking() {
    Timer?.Kill();
    Timer = null;
  }

  public virtual void StartTracking(CCSPlayerController? player = null,
    float? updateRate = null) {
    UpdateRate = updateRate ?? UpdateRate;
    if (player != null) Player = player;
    Timer?.Kill();
    Timer = Plugin.AddTimer(UpdateRate, Tick,
      TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
  }

  public override void Kill() {
    foreach (var segment in Segments) segment.Remove();

    StopTracking();
  }
}