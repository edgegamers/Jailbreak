using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.Mute;

public interface IMuteService
{
    void PeaceMute(MuteReason reason);
    void RemovePeaceMute();
    bool IsPeaceEnabled();
    DateTime GetLastPeace();
}