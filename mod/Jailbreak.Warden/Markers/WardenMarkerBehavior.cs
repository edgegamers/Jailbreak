using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.Warden;

namespace Jailbreak.Warden.Markers;

public class WardenMarkerBehavior : IPluginBehavior
{
    private readonly IWardenService _warden;
    private IDrawService _drawer;

    private DrawableShape? _marker;

    public WardenMarkerBehavior(IWardenService warden, IDrawService drawer)
    {
        _warden = warden;
        _drawer = drawer;
    }

    public void Start(BasePlugin plugin)
    {
        _marker = new BeamCircle(plugin, new Vector(), 40f, (int)Math.PI * 10);
        plugin.AddCommandListener("player_ping", CommandListener_PlayerPing);
    }

    [GameEventHandler]
    public HookResult OnPing(EventPlayerPing @event, GameEventInfo info)
    {
        var player = @event.Userid;

        if (!_warden.IsWarden(player))
            return HookResult.Handled;

        Vector vec = new Vector(@event.X, @event.Y, @event.Z);
        _marker?.Move(vec);
        _marker?.Update();
        return HookResult.Handled;
    }

    HookResult CommandListener_PlayerPing(CCSPlayerController? player, CommandInfo info)
    {
        return HookResult.Handled;
    }
}