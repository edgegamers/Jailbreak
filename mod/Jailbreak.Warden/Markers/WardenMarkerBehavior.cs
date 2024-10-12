using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Warden;
using MStatsShared;

namespace Jailbreak.Warden.Markers;

public class WardenMarkerBehavior(IWardenService warden)
  : IPluginBehavior, IMarkerService {
  public static readonly FakeConVar<float> CV_MAX_RADIUS = new(
    "css_jb_warden_marker_max_radius", "Maximum radius for warden marker", 360);

  public static readonly FakeConVar<float> CV_MIN_RADIUS = new(
    "css_jb_warden_marker_min_radius", "Minimum radius for warden marker", 60);

  public static readonly FakeConVar<long> CV_RESIZE_TIME = new(
    "css_jb_warden_resize_time", "Milliseconds to wait for resizing marker",
    800);

  // private Vector? MarkerPosition;

  private BeamCircle? marker;
  private long placementTime;

  public Vector? MarkerPosition { get; private set; }

  public float radius { get; private set; }
  // private float radius;

  public void Start(BasePlugin basePlugin) {
    marker = new BeamCircle(basePlugin, new Vector(), CV_MIN_RADIUS.Value,
      (int)Math.PI * 15);
    basePlugin.AddCommandListener("player_ping", CommandListener_PlayerPing);
  }

  [GameEventHandler]
  public HookResult OnPing(EventPlayerPing @event, GameEventInfo info) {
    var player = @event.Userid;

    if (!warden.IsWarden(player)) return HookResult.Handled;
    var vec = new Vector(@event.X, @event.Y, @event.Z);

    if (MarkerPosition != null) {
      var distance = MarkerPosition.Distance(vec);
      var timeElapsed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        - placementTime;
      if (timeElapsed < CV_RESIZE_TIME.Value) {
        if (distance <= CV_MAX_RADIUS.Value * 1.3) {
          distance = Math.Clamp(distance, CV_MIN_RADIUS.Value,
            CV_MAX_RADIUS.Value);
          marker?.SetRadius(distance);
          marker?.Update();
          radius = distance;
          return HookResult.Handled;
        }
      } else if (distance <= radius) {
        marker?.Remove();
        return HookResult.Handled;
      }
    }

    radius         = CV_MIN_RADIUS.Value;
    MarkerPosition = vec;
    placementTime  = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    API.Stats?.PushStat(new ServerStat("JB_MARKER",
      $"{vec.X:F2} {vec.Y:F2} {vec.Z:F2}"));
    marker?.Move(vec);
    marker?.SetRadius(radius);
    marker?.Update();
    return HookResult.Handled;
  }

  private HookResult CommandListener_PlayerPing(CCSPlayerController? player,
    CommandInfo info) {
    return warden.IsWarden(player) ? HookResult.Continue : HookResult.Handled;
  }
}