using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Generic;
using Jailbreak.Public.Mod.Plugin;
using Jailbreak.Public.Mod.Warden;
using static Jailbreak.Public.Mod.Warden.PeaceMuteOptions;

namespace Jailbreak.Warden.Global;

/// <summary>
/// This class defines functionality for the "peace-mute". Peace-mute referrs to any of the following: 
/// Start of round prisoner mute, css_peace command mute, and first-warden mute.
/// Any functions that peace-mutes players will check if the target player is previously muted or not, to respect already-muted players.
/// Any unmute functions also try to respect this.
/// There are a number of event listeners to handle edge cases, such as removing peace-mute when the round ends, or more general things such as 
/// enabling peace-mute for Terrorists when the round begins.
/// If a player is alive they should either be able to speak unless peace-mute is active or an admin-mute is active.
/// If a player is dead they should never be able to speak to alive players, and their admin-mute status should be respected.
/// </summary>
public class WardenPeaceBehaviour : IPluginBehavior, IWardenPeaceService
{

    private readonly IWardenService _wardenService;
    private readonly ICoroutines _coroutines;
    private readonly IEventsService _eventsService;
    private readonly IWardenPeaceNotifications _wardenPeaceNotifications;

    /// <summary>
    /// Defines a list of players that are muted, respective of the team. 
    /// This Dictionary should be empty when there are no players muted (i.e. peace-mute is not active)
    /// and non-empty otherwise. The _peaceMuteActive boolean field should reflect this.
    /// </summary>
    private readonly Dictionary<CsTeam, List<CCSPlayerController>> _currentlyMutedAlivePlayers;
    /// <summary>
    /// This list ensures the unmuting of players targetted by peace-mute at any point in the round
    /// are only alive.
    /// </summary>
    private readonly List<CCSPlayerController> _deadPlayersAndSpectators;

    private bool _roundEnd;

    // TODO move these to configs?
    public static readonly float _commandMuteTime = 10.0f;
    public static readonly float _startRoundPrisonerMuteTime = 45.0f;
    public static readonly string _mutedFlag = "@css/muted"; // would it be better to make this a group rather than a permission ? 

    public WardenPeaceBehaviour(IWardenService wardenService, ICoroutines coroutines, IEventsService eventsService, IWardenPeaceNotifications wardenPeaceNotifications)
    {

        _wardenService = wardenService;
        _coroutines = coroutines;
        _eventsService = eventsService;
        _wardenPeaceNotifications = wardenPeaceNotifications;
        _currentlyMutedAlivePlayers = new Dictionary<CsTeam, List<CCSPlayerController>>();
        _deadPlayersAndSpectators = new List<CCSPlayerController>();
        
        _roundEnd = false;

        // makes it so we don't have to try key values that aren't in the dictionary
        _currentlyMutedAlivePlayers.Add(CsTeam.None, new List<CCSPlayerController>());
        _currentlyMutedAlivePlayers.Add(CsTeam.Terrorist, new List<CCSPlayerController>());
        _currentlyMutedAlivePlayers.Add(CsTeam.CounterTerrorist, new List<CCSPlayerController>());
        _currentlyMutedAlivePlayers.Add(CsTeam.Spectator, new List<CCSPlayerController>());

        Func<bool> firstWardenPeaceMuteCallback = () =>
        {
            PeaceMuteOptions options = new PeaceMuteOptions(MuteReason.FIRSTWARDEN, _commandMuteTime, CsTeam.Terrorist, CsTeam.CounterTerrorist);
            PeaceMute(options);
            return true;
        };

        _eventsService.RegisterEventListener("first_warden_event", firstWardenPeaceMuteCallback);
    
    }

    public bool IsWarden(CCSPlayerController? player)
    {
        return _wardenService.IsWarden(player);
    }

    public void PeaceMute(PeaceMuteOptions options)
    {

        float time = options.GetTimeSeconds();
        MuteReason reason = options.GetReason(); // startround
        CsTeam[] targets = options.GetTargetTeams(); // terrorists

        foreach (CCSPlayerController player in Utilities.GetPlayers())
        {
            foreach (CsTeam target in targets)
            {

                if (_wardenService.IsWarden(player)) // always exempt warden
                    continue;

                if (AdminManager.PlayerHasPermissions(player, "@css/generic")) // always exempt admins as well
                    continue;

                // want to ignore already muted players...
                if (player.VoiceFlags == VoiceFlags.Muted || AdminManager.PlayerHasPermissions(player, _mutedFlag))
                    continue;


                if ((player.GetTeam() & target) != target) // exclude any player not in the target team
                    continue;

                // finally we know the player in question is not an admin or the warden, and does not already have an admin-mute 
                // so we can safely mute them here
                player.VoiceFlags |= VoiceFlags.Muted;
                _currentlyMutedAlivePlayers[target].Add(player);

            }
        }

        // resolve the mute reason
        switch (reason)
        {
            case MuteReason.CSS_PEACE:
                _wardenPeaceNotifications.PLAYERS_MUTED_VIACMD.ToAllChat(); break;
            case MuteReason.FIRSTWARDEN:
                _wardenPeaceNotifications.PLAYERS_MUTED_FIRSTWARDEN.ToAllChat(); break;
            case MuteReason.PRISONERS_STARTROUND:
                _wardenPeaceNotifications.PRISONERS_MUTED_STARTROUND.ToAllChat(); break;
        }

        // then unmute the people who weren't already muted after _muteTime seconds
        _coroutines.Round(() =>
        {
            // if there are no active mutes anymore this function will do nothing.
            UnmutePrevMutedAlivePlayers(reason, targets);

        }, time);
    }
    
    /// <summary>
    /// Ensures start of round prisoner mute is enabled.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        _roundEnd = false;
        PeaceMuteOptions options = new PeaceMuteOptions(MuteReason.PRISONERS_STARTROUND, _startRoundPrisonerMuteTime, CsTeam.Terrorist);
        PeaceMute(options);

        return HookResult.Continue;
    }

    /// <summary>
    /// Ensures that if peace-mute is active when the round starts then any players that join the Terrorist team
    /// will still be muted!
    /// </summary>
    /// <param name="event"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    [GameEventHandler]
    public HookResult OnPlayerJoinTeam(EventPlayerTeam @event, GameEventInfo info)
    {

        if (!@event.Userid.IsValid) { return HookResult.Continue; }

        // I hope this means they are automatically either a Terrorist or a CounterTerrorist...
        if (@event.Userid.PawnIsAlive)
        {
            _deadPlayersAndSpectators.Remove(@event.Userid); // just in case!
            // ignore muted players
            if (AdminManager.PlayerHasPermissions(@event.Userid, _mutedFlag)) { return HookResult.Continue; }

            CsTeam team = @event.Userid.GetTeam();
            if (!IsMuteActiveInTeam(team)) { return HookResult.Continue; }

            @event.Userid.VoiceFlags |= VoiceFlags.Muted; // actually set them muted!
            _currentlyMutedAlivePlayers[team].Add(@event.Userid); // the list is automatically cleared after some conditions

        }
        else // they are dead and get added to dead/spectator list regardless
        {
            _deadPlayersAndSpectators.Add(@event.Userid);
            @event.Userid.VoiceFlags |= VoiceFlags.Muted;
        }

        return HookResult.Continue;

    }

    /// <summary>
    /// Ensures we clear-up if a player disconnects while in a peace-muted team.
    /// If the player were to join back they should be automatically on the spectators/dead players list.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    [GameEventHandler]
    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {

        if (!@event.Userid.IsValid) { return HookResult.Continue; }
        CsTeam playerTeam = @event.Userid.GetTeam();

        if (!IsMuteActiveInTeam(playerTeam)) { return HookResult.Continue; }
        // todo I need to check if voice flags persist on players leaving the server

        _currentlyMutedAlivePlayers[playerTeam].Remove(@event.Userid);
        _deadPlayersAndSpectators.Remove(@event.Userid);

        return HookResult.Continue;

    }

    /// <summary>
    /// Ensures peace-mute is disabled as this could have issues such as the muting of players carrying over into the next round,
    /// or simply not being able to hear others when the round ends.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    [GameEventHandler]
    public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        _roundEnd = true;
        UnmutePrevMutedAlivePlayers(MuteReason.END_ROUND, CsTeam.Terrorist, CsTeam.CounterTerrorist, CsTeam.Spectator, CsTeam.None);
        UnmutePrevMutedDeadPlayers();
        return HookResult.Continue;

    }

    /// <summary>
    /// Handles dead players and spectators, as discussed in the _deadPlayersAndSpectators field.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    
    // TODO WE DON'T WANT TO MUTE THE PLAYER WE WANT TO MAKE IT SO THAT ALIVE PLAYERS CAN'T HEAR DEAD ONES
    // ALSO WE WANT TO HANDLE WHEN A PLAYER DIES
    [GameEventHandler]
    public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        if (!@event.Userid.IsValid) { return HookResult.Continue; } // check player handle exists first

        // so that dead players are not affected by other function calls
        if (IsMuteActiveInTeam(@event.Userid.GetTeam())) { _currentlyMutedAlivePlayers[@event.Userid.GetTeam()].Remove(@event.Userid); }
        _deadPlayersAndSpectators.Add(@event.Userid);

        if (AdminManager.PlayerHasPermissions(@event.Userid, _mutedFlag)) { return HookResult.Continue; }

        // mute dead players
        @event.Userid.VoiceFlags |= VoiceFlags.Muted; return HookResult.Continue; 

    }

    public void UnmutePrevMutedAlivePlayers(MuteReason reason, params CsTeam[] targets)
    {

        if (!IsMuteActiveInTeams(targets)) { return; } // if a mute isn't active in at least one of these targets, we don't want to trigger an unmute message.

        foreach (CsTeam target in targets)
        {
            List<CCSPlayerController> playersToUnmute = _currentlyMutedAlivePlayers[target];
            foreach (CCSPlayerController player in playersToUnmute)
            {
                if (!player.IsValid || !player.PawnIsAlive) continue; // handled by _roundEnd
                if (_deadPlayersAndSpectators.Contains(player)) continue; // we don't want to unmute dead players/spectators... handled by _roundEnd

                if (AdminManager.PlayerHasPermissions(player, _mutedFlag)) { continue; } // don't unmute admin-muted players!
                /* If this flag isn't set, then the admin-muted players get unmuted anyway :( */

                player.VoiceFlags &= ~VoiceFlags.Muted;

            }
            playersToUnmute.Clear();
        }

        // resolve the unmute reason 
        switch (reason)
        {
            case MuteReason.CSS_PEACE:
                _wardenPeaceNotifications.PLAYERS_UNMUTED_VIACMD.ToAllChat(); break;
            case MuteReason.FIRSTWARDEN:
                _wardenPeaceNotifications.PLAYERS_UNMUTED_FIRSTWARDEN.ToAllChat(); break;
            case MuteReason.PRISONERS_STARTROUND:
                _wardenPeaceNotifications.PRISONERS_UNMUTED_STARTROUND.ToAllChat(); break;
            case MuteReason.WARDEN_DIED:
                _wardenPeaceNotifications.PLAYERS_WARDEN_DIED.ToAllChat(); break;
            case MuteReason.ADMIN_REMOVED_PEACEMUTE:
                _wardenPeaceNotifications.PLAYERS_UNMUTED_ADMINCMD.ToAllChat(); break;
            case MuteReason.END_ROUND:
                _wardenPeaceNotifications.PLAYERS_UNMUTED_ROUNDEND.ToAllChat(); break;
        }

    }

    public void UnmutePrevMutedDeadPlayers()
    {
        if (_roundEnd)
        {
            foreach (CCSPlayerController player in _deadPlayersAndSpectators)
            {
                if (!player.IsValid) { continue; }
                if (AdminManager.PlayerHasPermissions(player, _mutedFlag)) { continue; } // don't unmute admin-muted players!
                player.VoiceFlags &= ~VoiceFlags.Muted;
            }
        }
        _deadPlayersAndSpectators.Clear();
    }

    public bool IsMuteActiveInTeam(CsTeam team)
    {
        return _currentlyMutedAlivePlayers[team].Count != 0;
    }

    public bool IsMuteActiveInTeams(params CsTeam[] teams)
    {
        foreach (CsTeam team in teams)
        {
            if (_currentlyMutedAlivePlayers[team].Count > 0) return true;
        }

        return false;

    }

}
