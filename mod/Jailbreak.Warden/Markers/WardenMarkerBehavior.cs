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

public class WardenMarkerBehavior(IWardenService warden) : IPluginBehavior {
  public readonly FakeConVar<float> CvMaxRadius = new(
    "css_jb_warden_marker_max_radius", "Maximum radius for warden marker", 360);

  public readonly FakeConVar<float> CvMinRadius = new(
    "css_jb_warden_marker_min_radius", "Minimum radius for warden marker", 60);

  public readonly FakeConVar<long> CvResizeTime = new(
    "css_jb_warden_resize_time", "Milliseconds to wait for resizing marker",
    800);

  private Vector? currentPos;

  private BeamCircle? marker;
  private long placementTime;
  private float radius;

  public void Start(BasePlugin basePlugin) {
    marker = new BeamCircle(basePlugin, new Vector(), CvMinRadius.Value,
      (int)Math.PI * 15);
    basePlugin.AddCommandListener("player_ping", CommandListener_PlayerPing);
  }

  [GameEventHandler]
  public HookResult OnPing(EventPlayerPing @event, GameEventInfo info) {
    var player = @event.Userid;

    if (!warden.IsWarden(player)) return HookResult.Handled;
    var vec = new Vector(@event.X, @event.Y, @event.Z);

    if (currentPos != null) {
      var distance = currentPos.Distance(vec);
      var timeElapsed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        - placementTime;
      if (timeElapsed < CvResizeTime.Value) {
        if (distance <= CvMaxRadius.Value * 1.3) {
          distance = Math.Clamp(distance, CvMinRadius.Value, CvMaxRadius.Value);
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

    radius        = CvMinRadius.Value;
    currentPos    = vec;
    placementTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

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