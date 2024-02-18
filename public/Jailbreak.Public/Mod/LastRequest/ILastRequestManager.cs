using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.Public.Mod.LastRequest;

public interface ILastRequestManager : IPluginBehavior
{
    public bool IsLREnabled { get; set; }
    public IList<AbstractLastRequest> ActiveLRs { get; }
    
    void InitiateLastRequest(CCSPlayerController guard, CCSPlayerController prisoner, LRType lrType);

    public bool IsInLR(CCSPlayerController player)
    {
        return ActiveLRs.Any(lr => lr.guard.Slot == player.Slot || lr.prisoner.Slot == player.Slot);
    }
    
}