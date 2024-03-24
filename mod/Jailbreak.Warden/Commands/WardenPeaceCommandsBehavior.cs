using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.Warden;
using Jailbreak.Public.Mod.Plugin;
using Jailbreak.Warden.Global;
using Jailbreak.Formatting.Views;
using Jailbreak.Formatting.Extensions;
using static Jailbreak.Public.Mod.Warden.PeaceMuteOptions;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Warden.Commands;

public class WardenPeaceCommandsBehavior : IPluginBehavior
{

    private readonly IWardenPeaceService _peaceService;
    private readonly IWardenPeaceNotifications _wardenPeaceNotifications;
    private readonly IEventsService _eventsService;

    private DateTime _lastUsedTime;

    public WardenPeaceCommandsBehavior(IWardenPeaceService peaceService, IEventsService eventsService, IWardenPeaceNotifications wardenPeaceNotifications)
    {
        _peaceService = peaceService;
        _eventsService = eventsService;
        _wardenPeaceNotifications = wardenPeaceNotifications;

        Func<bool> wardenDeathCallback = () =>
        {
            _peaceService.UnmutePrevMutedAlivePlayers(MuteReason.WARDEN_DIED, CsTeam.Terrorist, CsTeam.CounterTerrorist);
            return true;
        };

        _eventsService.RegisterEventListener("warden_death_event", wardenDeathCallback);

    }

    [ConsoleCommand("css_peace", "Gives everybody some peace of mind by muting Prisoners/Guards for 10 seconds (warden is exempt from this).")]
    [CommandHelper(0, "", CommandUsage.CLIENT_ONLY)]
    public void Command_Peace(CCSPlayerController? invoker, CommandInfo command)
    {

        if (invoker == null) return;

        // we only want the warden OR ADMIN to be able to run this!
        if (!_peaceService.IsWarden(invoker) || !AdminManager.PlayerHasPermissions(invoker, "@css/generic")) return;

        // we still need if css_peace command mute is active in a team because it may have ended because of various things but the cooldown wouldn't have expired yet.
        if (_peaceService.IsMuteActiveInTeam(CsTeam.CounterTerrorist) && (DateTime.Now - _lastUsedTime).Seconds < WardenPeaceBehaviour._commandMuteTime)
        {
            _wardenPeaceNotifications.CSS_PEACE_COOLDOWN(WardenPeaceBehaviour._commandMuteTime - (DateTime.Now - _lastUsedTime).Seconds).ToAllChat();
            return;
        }

        PeaceMuteOptions options = new PeaceMuteOptions(MuteReason.CSS_PEACE, WardenPeaceBehaviour._commandMuteTime, CsTeam.Terrorist, CsTeam.CounterTerrorist);
        _peaceService.PeaceMute(options);
        _lastUsedTime = DateTime.Now;
         
    }

    [ConsoleCommand("css_unpeace", "Lets the admins remove the warden mute.")]
    [CommandHelper(0, "", CommandUsage.CLIENT_ONLY)]
    [RequiresPermissionsOr("@css/ban", "@css/kick", "@css/generic")]
    public void Command_UnPeace(CCSPlayerController? invoker, CommandInfo command)
    {
        if (invoker == null) return;
        // this does nothing to teams that don't have any mutes, see the documentation for UnmutePrevMutedPlayers
        _peaceService.UnmutePrevMutedAlivePlayers(MuteReason.ADMIN_REMOVED_PEACEMUTE, CsTeam.Terrorist, CsTeam.CounterTerrorist, CsTeam.Spectator, CsTeam.None);
    }

}
