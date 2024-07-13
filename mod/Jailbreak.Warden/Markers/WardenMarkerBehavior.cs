using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Warden;

namespace Jailbreak.Warden.Markers;

public class WardenMarkerBehavior(IWardenService warden) : IPluginBehavior {
  private const float MIN_RADIUS = 60f, MAX_RADIUS = 360f;

  private Vector? currentPos;

  private BeamCircle? marker;
  private long placementTime;
  private float radius = MIN_RADIUS;

  public void Start(BasePlugin basePlugin) {
    marker = new BeamCircle(basePlugin, new Vector(), 60f, (int)Math.PI * 15);
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
      if (timeElapsed < 500) {
        if (distance <= MAX_RADIUS * 1.5) {
          distance = Math.Clamp(distance, MIN_RADIUS, MAX_RADIUS);
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

    radius        = MIN_RADIUS;
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