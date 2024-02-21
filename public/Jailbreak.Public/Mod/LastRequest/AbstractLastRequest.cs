using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.Public.Mod.LastRequest;

public abstract class AbstractLastRequest
{
    public CCSPlayerController prisoner { get; protected set; }
    public CCSPlayerController guard { get; protected set; }
    public abstract LRType type { get; }

    public LRState state { get; protected set; }
    protected BasePlugin plugin;

    protected AbstractLastRequest(BasePlugin plugin, CCSPlayerController prisoner, CCSPlayerController guard)
    {
        this.plugin = plugin;
        this.prisoner = prisoner;
        this.guard = guard;
    }

    public void PrintToParticipants(string message)
    {
        prisoner.PrintToChat(message);
        guard.PrintToChat(message);
    }

    public abstract void Setup();
    public abstract void Execute();
    public abstract void End(LRResult result);
}