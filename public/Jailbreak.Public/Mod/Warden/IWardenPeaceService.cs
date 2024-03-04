
using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.Warden;

public interface IWardenPeaceService
{

    public CCSPlayerController? GetWarden();

    // todo document saying that by default all admins SHOULD bypass this mute
    // not implemented bypass yet
    public void PeaceMute(float time, bool exemptWarden = false);

}
