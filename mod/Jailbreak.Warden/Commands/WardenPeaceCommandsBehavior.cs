using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.Warden;
using Jailbreak.Public.Generic;
using Jailbreak.Public.Mod.Plugin;
using CounterStrikeSharp.API;
using Jailbreak.Warden.Paint;
using Jailbreak.Warden.Global;

namespace Jailbreak.Warden.Commands;

public class WardenPeaceCommandsBehavior : IPluginBehavior
{

    private readonly IWardenPeaceService _peaceService;
    private readonly ICoroutines _coroutines;
    private readonly IEventsService _eventsService;

    private bool _peaceCommandCooldown = false;

    public WardenPeaceCommandsBehavior(IWardenPeaceService peaceService, ICoroutines coroutines, IEventsService eventsService)
    {
        _peaceService = peaceService;
        _coroutines = coroutines;
        _eventsService = eventsService;

        Func<bool> wardenDeathCallback = () =>
        {
            _peaceCommandCooldown = false;
            return true;
        };

        _eventsService.RegisterEventListener("warden_death_event", wardenDeathCallback);

    }

    [ConsoleCommand("css_peace", "Gives everybody some peace of mind by muting Prisoners/Guards for x seconds (warden is exempt from this).")]
    [CommandHelper(0, "", CommandUsage.CLIENT_ONLY)]
    public void Command_Peace(CCSPlayerController? invoker, CommandInfo command)
    {

        if (invoker == null) return;

        if (_peaceCommandCooldown)
        {
            invoker.PrintToChat("peace cmd still active, i.e. you're on a cooldown"); // placeholder
            return;
        }

        // we only want the warden to be able to run this!
        if (!_peaceService.IsWarden(invoker)) return;

        _peaceService.PeaceMute(WardenPeaceBehaviour._muteTime);

        _peaceCommandCooldown = true;
        _coroutines.Round(() => _peaceCommandCooldown = false, WardenPeaceBehaviour._muteTime); // command cooldown

    }

}
