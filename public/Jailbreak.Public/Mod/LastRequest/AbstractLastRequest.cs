using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.Public.Mod.LastRequest;

public abstract class AbstractLastRequest
{
    public CCSPlayerController prisoner { get; protected set; }
    public CCSPlayerController guard { get; protected set; }
    public LRType type { get; protected set; }
    public LRState state { get; protected set; }
    protected BasePlugin plugin;
    protected DateTimeOffset startTime;

    protected AbstractLastRequest(BasePlugin plugin, CCSPlayerController prisoner, CCSPlayerController guard)
    {
        this.plugin = plugin;
        this.prisoner = prisoner;
        this.guard = guard;
    }


    public abstract void Setup();
    public abstract void Execute();
    public abstract void End(LRResult result);
}