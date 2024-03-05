
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Mod.Warden;

public interface IWardenPeaceService
{

    public bool IsWarden(CCSPlayerController? player);
    // todo document saying that by default all admins SHOULD bypass this mute
    // not implemented bypass yet

    // the peacemute function automatically sets the _peaceService.SetPeaceMuteActive() function to true
    public void PeaceMute(PeaceMuteOptions options);

    public bool GetPeaceMuteActive();

    public void UnmutePrevMutedPlayers(PeaceMuteOptions.MuteReason reason, params CsTeam[] target);

}
