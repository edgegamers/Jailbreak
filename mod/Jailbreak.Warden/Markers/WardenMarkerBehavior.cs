using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Warden;

namespace Jailbreak.Warden.Markers;

public class WardenMarkerBehavior : IPluginBehavior {
  private const float MinRadius = 60f, MaxRadius = 360f;
  private readonly IWardenService _warden;

  private Vector? _currentPos;

  private BeamCircle? _marker;
  private long _placementTime;
  private float _radius = MinRadius;

  public WardenMarkerBehavior(IWardenService warden) { _warden = warden; }

  public void Start(BasePlugin plugin) {
    _marker = new BeamCircle(plugin, new Vector(), 60f, (int)Math.PI * 15);
    plugin.AddCommandListener("player_ping", CommandListener_PlayerPing);
  }

  [GameEventHandler]
  public HookResult OnPing(EventPlayerPing @event, GameEventInfo info) {
    var player = @event.Userid;

    if (!_warden.IsWarden(player)) return HookResult.Handled;
    var vec = new Vector(@event.X, @event.Y, @event.Z);

    if (_currentPos != null) {
      var distance = _currentPos.Distance(vec);
      var timeElapsed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        - _placementTime;
      if (timeElapsed < 500) {
        if (distance <= MaxRadius * 1.5) {
          distance = Math.Clamp(distance, MinRadius, MaxRadius);
          _marker?.SetRadius(distance);
          _marker?.Update();
          _radius = distance;
          return HookResult.Handled;
        }
      } else if (distance <= _radius) {
        _marker?.Remove();
        return HookResult.Handled;
      }
    }

    _radius        = MinRadius;
    _currentPos    = vec;
    _placementTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    _marker?.Move(vec);
    _marker?.SetRadius(_radius);
    _marker?.Update();
    return HookResult.Handled;
  }

  private HookResult CommandListener_PlayerPing(CCSPlayerController? player,
    CommandInfo info) {
    return _warden.IsWarden(player) ? HookResult.Continue : HookResult.Handled;
  }
}