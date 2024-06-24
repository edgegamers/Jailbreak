using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.LastGuard;

public interface ILastGuardService
{
    bool IsLastGuard();
    void StartLastGuard(CCSPlayerController guard);
    void EndLastGuard();
}