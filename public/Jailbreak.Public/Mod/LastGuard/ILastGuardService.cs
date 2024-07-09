using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.LastGuard;

public interface ILastGuardService
{
    int CalculateHealth();
    void StartLastGuard(CCSPlayerController lastGuard);
}