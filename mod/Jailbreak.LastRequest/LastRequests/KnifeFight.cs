using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.LastRequest.LastRequests;

public class KnifeFight : AbstractLastRequest
{
    public KnifeFight(BasePlugin plugin, CCSPlayerController prisoner, CCSPlayerController guard) : base(plugin,
        prisoner, guard)
    {
    }

    public override void Setup()
    {
        // Strip weapons, teleport T to CT
    }

    public override void Execute()
    {
        // Give knives 
    }

    public override void End(LRResult result)
    {
        // Slay the loser
    }
}