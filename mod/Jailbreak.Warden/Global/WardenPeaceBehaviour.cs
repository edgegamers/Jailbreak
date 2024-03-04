using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Generic;
using Jailbreak.Public.Mod.Plugin;
using Jailbreak.Public.Mod.Warden;

namespace Jailbreak.Warden.Global;

public class WardenPeaceBehaviour : IPluginBehavior, IWardenPeaceService
{

    private readonly IWardenService _wardenService;
    private readonly ICoroutines _coroutines;
    private readonly IEventsService _eventsService;

    private readonly float _muteTime = 10.0f;

    public WardenPeaceBehaviour(IWardenService wardenService, ICoroutines coroutines, IEventsService eventsService)
    {
        _wardenService = wardenService;
        _coroutines = coroutines;
        _eventsService = eventsService;

        Func<bool> firstWardenPeaceMuteCallback = () =>
        {
            PeaceMute(_muteTime, true);
            return true;
        };

        _eventsService.RegisterEventListener("first_warden_event", firstWardenPeaceMuteCallback);
    
    }

    public CCSPlayerController? GetWarden()
    {
        return _wardenService.Warden;
    }

    public void PeaceMute(float time, bool exemptWarden = false)
    {

        List<CCSPlayerController> alreadyMuted = new List<CCSPlayerController>();
        List<CCSPlayerController> prevUnmutedPlayers = new List<CCSPlayerController>();

        foreach (CCSPlayerController player in Utilities.GetPlayers())
        {

            if (player.Equals(_wardenService.Warden) && exemptWarden)
                continue;

            if (player.VoiceFlags == VoiceFlags.Muted)
            {
                player.PrintToChat("bro you already muted");
                alreadyMuted.Add(player);
            }
            else
            {
                player.VoiceFlags |= VoiceFlags.Muted;
                prevUnmutedPlayers.Add(player);
                player.PrintToChat("we muted you");
            }

        }

        // then unmute the people who weren't already muted after _muteTime seconds
        _coroutines.Round(() =>
        {

            foreach (CCSPlayerController player in prevUnmutedPlayers)
            {
                player.VoiceFlags &= ~VoiceFlags.Muted;
                player.PrintToChat("we've unmuted you");
            }

        }, time);
    }


}
