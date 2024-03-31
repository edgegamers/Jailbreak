using CounterStrikeSharp.API.Core;
using Jailbreak.LastRequest.LastRequests;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.LastRequest;

public class LastRequestFactory(ILastRequestManager manager) : ILastRequestFactory
{
    private BasePlugin _plugin;

    public void Start(BasePlugin parent)
    {
        _plugin = parent;
    }

    public AbstractLastRequest CreateLastRequest(CCSPlayerController prisoner, CCSPlayerController guard, LRType type)
    {
        return type switch
        {
            LRType.KnifeFight => new KnifeFight(_plugin, manager, prisoner, guard),
            LRType.GunToss => new GunToss(_plugin, manager, prisoner, guard),
            LRType.NoScope => new NoScope(_plugin, manager, prisoner, guard),
            LRType.RockPaperScissors => new RockPaperScissors(_plugin, manager, prisoner, guard),
            LRType.Coinflip => new Coinflip(_plugin, manager, prisoner, guard),
            LRType.Race => new Race(_plugin, manager, prisoner, guard),
            _ => throw new ArgumentException("Invalid last request type: " + type, nameof(type))
        };
    }

    public bool IsValidType(LRType type)
    {
        return type switch
        {
            LRType.KnifeFight => true,
            LRType.GunToss => true,
            LRType.NoScope => true,
            LRType.RockPaperScissors => true,
            LRType.Coinflip => true,
            LRType.Race => true,
            _ => false
        };
    }
}