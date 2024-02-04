using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Warden;

namespace Jailbreak.Warden.Markers;

public class WardenMarkerBehavior : IPluginBehavior
{
    private readonly IWardenService _warden;

    private BeamCircle? _marker;

    private Vector? currentPos;
    private const float MIN_RADIUS = 60f, MAX_RADIUS = 360f;
    private float radius = MIN_RADIUS;
    private long placementTime = 0;

    public WardenMarkerBehavior(IWardenService warden)
    {
        _warden = warden;
    }

    public void Start(BasePlugin plugin)
    {
        _marker = new BeamCircle(plugin, new Vector(), 60f, (int)Math.PI * 15);
        plugin.AddCommandListener("player_ping", CommandListener_PlayerPing);
    }

    [GameEventHandler]
    public HookResult OnPing(EventPlayerPing @event, GameEventInfo info)
    {
        var player = @event.Userid;

        if (!_warden.IsWarden(player))
            return HookResult.Handled;
        Vector vec = new Vector(@event.X, @event.Y, @event.Z);

        if (currentPos != null)
        {
            float distance = currentPos.Distance(vec);
            long timeElapsed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - placementTime;
            if (timeElapsed < 500)
            {
                if (distance <= MAX_RADIUS * 1.5)
                {
                    distance = Math.Clamp(distance, MIN_RADIUS, MAX_RADIUS);
                    _marker?.SetRadius(distance);
                    _marker?.Update();
                    radius = distance;
                    return HookResult.Handled;
                }
            }
            else if (distance <= radius)
            {
                _marker?.Remove();
                return HookResult.Handled;
            }
        }

        radius = MIN_RADIUS;
        currentPos = vec;
        placementTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        _marker?.Move(vec);
        _marker?.SetRadius(radius);
        _marker?.Update();
        return HookResult.Handled;
    }

    HookResult CommandListener_PlayerPing(CCSPlayerController? player, CommandInfo info)
    {
        return _warden.IsWarden(player) ? HookResult.Continue : HookResult.Handled;
    }
}