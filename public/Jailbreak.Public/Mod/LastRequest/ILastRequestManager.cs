using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.Public.Mod.LastRequest;

public interface ILastRequestManager : IPluginBehavior
{
    public bool IsLREnabled { get; set; }
    public IList<AbstractLastRequest> ActiveLRs { get; }

    bool InitiateLastRequest(CCSPlayerController prisoner, CCSPlayerController guard, LRType lrType);
    bool EndLastRequest(AbstractLastRequest lr, LRResult result);

    public bool IsInLR(CCSPlayerController player)
    {
        return GetActiveLR(player) != null;
    }

    public AbstractLastRequest? GetActiveLR(CCSPlayerController player)
    {
        return ActiveLRs.FirstOrDefault(lr => lr.guard.Slot == player.Slot || lr.prisoner.Slot == player.Slot);
    }
}