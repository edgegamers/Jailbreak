

using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.Warden;
using CounterStrikeSharp.API.Modules.Admin;
using Jailbreak.Formatting.Views;
using Jailbreak.Formatting.Extensions;
using static Jailbreak.Public.Mod.Warden.PeaceMuteOptions;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Warden.Commands;

public class WardenFireCommandBehavior : IPluginBehavior
{

    private readonly IWardenService _wardenService;
    private readonly IWardenPeaceService _wardenPeaceService;
    private readonly IWardenFireNotifications _wardenFireNotifications;

    public WardenFireCommandBehavior(IWardenService wardenService, IWardenPeaceService wardenPeaceService, IWardenFireNotifications wardenFireNotifications)
    {
        _wardenService = wardenService;
        _wardenPeaceService = wardenPeaceService;
        _wardenFireNotifications = wardenFireNotifications;
    }

    [ConsoleCommand("css_fire", "Attempts to fire the current warden.")]
    [CommandHelper(0, "", CommandUsage.CLIENT_ONLY)]
    [RequiresPermissionsOr("@css/generic")]
    public void Command_Fire(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
            return;

        if (_wardenService.HasWarden)
        {
            _wardenService.TryRemoveWarden();
            _wardenPeaceService.UnmutePrevMutedPlayers(MuteReason.ADMIN_REMOVED_PEACEMUTE, CsTeam.Terrorist, CsTeam.CounterTerrorist);
            _wardenFireNotifications.WARDEN_FIRED.ToAllChat().ToAllCenter();
        }

    }

}
