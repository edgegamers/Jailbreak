using CounterStrikeSharp.API.Core;
using Jailbreak.LastRequest.LastRequests;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.LastRequest;

public class LastRequestFactory : ILastRequestFactory
{
    private BasePlugin plugin;
    private ILastRequestManager manager;

    public LastRequestFactory(ILastRequestManager manager)
    {
        this.manager = manager;
    }

    public void Start(BasePlugin parent)
    {
        plugin = parent;
    }

    public AbstractLastRequest CreateLastRequest(CCSPlayerController prisoner, CCSPlayerController guard, LRType type)
    {
        return type switch
        {
            LRType.KnifeFight => new KnifeFight(plugin, manager, prisoner, guard),
            LRType.GunToss => new GunToss(plugin, manager, prisoner, guard),
            LRType.NoScope => new NoScope(plugin, manager, prisoner, guard),
            LRType.RockPaperScissors => new RockPaperScissors(plugin, manager, prisoner, guard),
            _ => throw new ArgumentException("Invalid last request type: " + type, nameof(type))
        };
    }
}