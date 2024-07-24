using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Warden;

namespace Jailbreak.Warden.Markers;

public class WardenMarkerBehavior(IWardenService warden) : IPluginBehavior {
  private readonly FakeConVar<float> cvMinRadius = new(
    "css_jb_warden_marker_min_radius", "Minimum radius for warden marker", 60);

  private readonly FakeConVar<float> cvMaxRadius = new(
    "css_jb_warden_marker_max_radius", "Maximum radius for warden marker", 360);

  private readonly FakeConVar<long> cvResizeTime = new(
    "css_jb_warden_resize_time", "Milliseconds to wait for resizing marker",
    800);

  private Vector? currentPos;

  private BeamCircle? marker;
  private long placementTime;
  private float radius;

  public void Start(BasePlugin basePlugin) {
    marker = new BeamCircle(basePlugin, new Vector(), cvMinRadius.Value,
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
      if (timeElapsed < cvResizeTime.Value) {
        if (distance <= cvMaxRadius.Value * 1.3) {
          distance = Math.Clamp(distance, cvMinRadius.Value, cvMaxRadius.Value);
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

    radius        = cvMinRadius.Value;
    currentPos    = vec;
    placementTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
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