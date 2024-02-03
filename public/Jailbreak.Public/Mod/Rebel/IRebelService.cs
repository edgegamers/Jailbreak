using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.Rebel;

public interface IRebelService
{
    ISet<CCSPlayerController> GetActiveRebels();

    bool IsRebel(CCSPlayerController player)
    {
        return GetRebelTimeLeft(player) > 0;
    }

    float GetRebelTimeLeft(CCSPlayerController player);

    bool MarkRebel(CCSPlayerController player, float time);
    
    void UnmarkRebel(CCSPlayerController player);
}