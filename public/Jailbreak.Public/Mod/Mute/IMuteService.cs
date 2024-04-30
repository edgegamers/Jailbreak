using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.Mute;

public interface IMuteService
{
    void PeaceMute(MuteReason reason);

    void UnPeaceMute();

    bool IsPeaceEnabled();

    DateTime GetLastPeace();
}