using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.Warden;
using Jailbreak.Public.Generic;

namespace Jailbreak.Warden.Commands;

public class WardenPeaceCommandsBehavior : IPluginBehavior
{

    private readonly IWardenPeaceService _peaceService;
    private readonly ICoroutines _coroutines;

    private static readonly float _muteTime = 10.0f;

    public WardenPeaceCommandsBehavior(IWardenPeaceService peaceService, ICoroutines coroutines)
    {
        _peaceService = peaceService;
        _coroutines = coroutines;
    }

    [ConsoleCommand("css_peace", "Gives everybody some peace of mind by muting Prisoners/Guards for x seconds (warden is exempt from this).")]
    [CommandHelper(0, "", CommandUsage.CLIENT_ONLY)]
    public void Command_Peace(CCSPlayerController? invoker, CommandInfo command)
    {

        if (invoker == null) return;

        CCSPlayerController? warden = _peaceService.GetWarden();

        if (warden == null) return;

        // we only want the warden to be able to run this!
        if (!invoker.Equals(warden)) return;

        _peaceService.PeaceMute(_muteTime, true);

    }

}
