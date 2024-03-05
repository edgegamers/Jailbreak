using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands.Targeting;
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
    private readonly Dictionary<CsTeam, List<CCSPlayerController>> _currentlyMutedTeams;
    /// <summary>
    /// This list ensures the unmuting of players targetted by peace-mute at any point in the round
    /// are only alive.
    /// </summary>
    private readonly List<CCSPlayerController> _deadPlayersAndSpectators;
    
    /// <summary>
    /// Defines if ANY peace-mute is active (start of round prisoner mute, css_peace command mute, and first-warden mute)
    /// </summary>
    private bool _peaceMuteActive;

    // TODO move these to configs?
    public static readonly float _commandMuteTime = 10.0f;
    public static readonly float _startRoundPrisonerMuteTime = 45.0f;
    public static readonly string _mutedFlag = "@css/muted";

    public WardenPeaceBehaviour(IWardenService wardenService, ICoroutines coroutines, IEventsService eventsService, IWardenPeaceNotifications wardenPeaceNotifications)
    {

        _wardenService = wardenService;
        _coroutines = coroutines;
        _eventsService = eventsService;
        _wardenPeaceNotifications = wardenPeaceNotifications;
        _peaceMuteActive = false;
        _currentlyMutedTeams = new Dictionary<CsTeam, List<CCSPlayerController>>();
        _deadPlayersAndSpectators = new List<CCSPlayerController>();

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

        _peaceMuteActive = true;

        float time = options.GetTimeSeconds();
        MuteReason reason = options.GetReason();
        CsTeam[] targets = options.GetTargetTeams();

        foreach (CsTeam target in targets)
        { 

            if (!_currentlyMutedTeams.Keys.Contains(target))
            {
                _currentlyMutedTeams[target] = new List<CCSPlayerController>();
            }

        // we need this for the unmute part below
        List<CCSPlayerController> currentlyMutedPlayers = _currentlyMutedTeams[target];

            foreach (CCSPlayerController player in Utilities.GetPlayers())
            {

                if (_wardenService.IsWarden(player)) // always exempt warden
                    continue;


                // want to ignore already muted players...
                if (player.VoiceFlags == VoiceFlags.Muted)
                    continue;


                // i.e. if the current player is not in our range of targets, then continue!
                if ((player.GetTeam() & target) == CsTeam.None)
                    continue;


                player.VoiceFlags |= VoiceFlags.Muted;
                currentlyMutedPlayers.Add(player);

            }
        }

        ResolveMuteReason(reason);

        // then unmute the people who weren't already muted after _muteTime seconds
        _coroutines.Round(() =>
        {

            // this variable is always be up-to-date (should be..)
            if (!_peaceMuteActive) return;
           
            _peaceMuteActive = false;

            foreach (CsTeam team in targets)
            {
                foreach (CCSPlayerController player in _currentlyMutedTeams[team])
                {
                    player.VoiceFlags &= ~VoiceFlags.Muted;
                }
            }

            ResolveUnmuteReason(reason);

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
        PeaceMuteOptions options = new PeaceMuteOptions(MuteReason.PRISONERS_STARTROUND, _startRoundPrisonerMuteTime, CsTeam.Terrorist);
        PeaceMute(options);

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
        UnmutePrevMutedPlayers(MuteReason.END_ROUND, CsTeam.Terrorist, CsTeam.CounterTerrorist, CsTeam.Spectator, CsTeam.None);
        _deadPlayersAndSpectators.Clear(); // important...
        return HookResult.Continue;

    }

    /// <summary>
    /// Handles dead players and spectators, as discussed in the _deadPlayersAndSpectators field.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    [GameEventHandler]
    public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        if (!@event.Userid.IsValid) { return HookResult.Continue; }
        _deadPlayersAndSpectators.Add(@event.Userid);

        return HookResult.Continue;
    }

    public void UnmutePrevMutedPlayers(MuteReason reason, params CsTeam[] targets)
    {

        foreach (CsTeam target in targets) 
        {
            // ignore targets we're not interested in
            if (!_currentlyMutedTeams.Keys.Contains(target)) { continue; }

            List<CCSPlayerController> playersToUnmute = _currentlyMutedTeams[target];
            foreach (CCSPlayerController player in playersToUnmute)
            {
                if (!player.IsValid) continue; // important
                if (_deadPlayersAndSpectators.Contains(player)) continue; // we don't want to unmute dead players/spectators...
                if (AdminManager.PlayerHasPermissions(player, "@css/muted")) continue; // we certainly don't want to unmute admin-muted players! 

                /* muted players should be ignored by my peace service anyway, but there are edge cases where players are admin-muted after my command is executed.
                 * By having a flag set on them, we can ensure we never unmute admin-muted players.
                 * If this flag isn't set, then the admin-muted players get unmuted anyway :( */

                player.VoiceFlags &= ~VoiceFlags.Muted;

            }

            _currentlyMutedTeams.Remove(target); 

        }

        ResolveUnmuteReason(reason);
        if (_currentlyMutedTeams.Keys.Count == 0) { _peaceMuteActive = false; } // makes sense hopefully ? 

    }
    /// <summary>
    /// A list of peace-mute reasons that the IWardenPeaceService can "throw". Useful method that maps these reasons to INotifications.
    /// </summary>
    /// <param name="reason"></param>
    private void ResolveMuteReason(MuteReason reason)
    {
        switch (reason)
        {
            case MuteReason.CSS_PEACE:
                _wardenPeaceNotifications.PLAYERS_MUTED_VIACMD.ToAllChat(); break;
            case MuteReason.FIRSTWARDEN:
                _wardenPeaceNotifications.PLAYERS_MUTED_FIRSTWARDEN.ToAllChat(); break;
            case MuteReason.PRISONERS_STARTROUND:
                _wardenPeaceNotifications.PRISONERS_MUTED_STARTROUND.ToAllChat(); break;
        }
    }

    /// <summary>
    /// A list of peace-unmute reasons that the IWardenPeaceService can "throw". Useful method that maps these reasons to INotifications.
    /// </summary>
    /// <param name="reason"></param>
    private void ResolveUnmuteReason(MuteReason reason)
    {
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

    public bool GetPeaceMuteActive()
    {
        return _peaceMuteActive;
    }

}
