using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.Mute;

public interface IMuteService
{
    void PeaceMute(MuteReason reason);

    bool IsPeaceEnabled();

    DateTime GetLastPeace();
}