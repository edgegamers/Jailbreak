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

public class WardenPeaceBehaviour : IPluginBehavior, IWardenPeaceService
{

    private readonly IWardenService _wardenService;
    private readonly ICoroutines _coroutines;
    private readonly IEventsService _eventsService;
    private readonly IWardenPeaceNotifications _wardenPeaceNotifications; // todo rename _peaceNotifications

    private readonly Dictionary<CsTeam, List<CCSPlayerController>> _currentlyMutedTeams;
    private readonly List<CCSPlayerController> _deadPlayersAndSpectators;
    // this bool is specific to the css_peace command being active
    private bool _peaceMuteActive;

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

            // if we did some other action outside of this function
            // such as UnmutePrevUnmutedPlayers(), then don't try to unmute again.

            // these variables are modified by other functions that ensure the players are unmuted before setting these states
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

    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        PeaceMuteOptions options = new PeaceMuteOptions(MuteReason.PRISONERS_STARTROUND, _startRoundPrisonerMuteTime, CsTeam.Terrorist);
        PeaceMute(options);

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        UnmutePrevMutedPlayers(MuteReason.END_ROUND, CsTeam.Terrorist, CsTeam.CounterTerrorist, CsTeam.Spectator, CsTeam.None);
        _deadPlayersAndSpectators.Clear(); // important...
        return HookResult.Continue;

    }

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
            // don't wanna bother about targets we're uninterested in
            if (!_currentlyMutedTeams.Keys.Contains(target)) { continue; }

            List<CCSPlayerController> playersToUnmute = _currentlyMutedTeams[target];
            foreach (CCSPlayerController player in playersToUnmute)
            {
                if (!player.IsValid) continue; // important
                if (_deadPlayersAndSpectators.Contains(player)) continue; // we don't want to unmute dead players/spectators...
                if (AdminManager.PlayerHasPermissions(player, "@css/muted")) continue; // we certainly don't want to unmute admin-muted players! 

                // muted players should be ignored by my peace service anyway, but there are edge cases where players are admin-muted after my command is executed
                // by having a flag set on them, we can ensure we never unmute admin-muted players.

                // if this flag isn't set, then the admin-muted players get unmuted anyway :(

                player.VoiceFlags &= ~VoiceFlags.Muted;

            }

            _currentlyMutedTeams.Remove(target); 

        }

        ResolveUnmuteReason(reason);
        if (_currentlyMutedTeams.Keys.Count == 0) { _peaceMuteActive = false; } // makes sense hopefully ? 

    }

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
