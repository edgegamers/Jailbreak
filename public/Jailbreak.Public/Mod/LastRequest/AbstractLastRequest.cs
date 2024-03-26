using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.Public.Mod.LastRequest;

public abstract class AbstractLastRequest(
    BasePlugin plugin,
    ILastRequestManager manager,
    CCSPlayerController prisoner,
    CCSPlayerController guard)
{
    public CCSPlayerController prisoner { get; protected set; } = prisoner;
    public CCSPlayerController guard { get; protected set; } = guard;
    public abstract LRType type { get; }

    public LRState state { get; protected set; }
    protected BasePlugin plugin = plugin;
    protected ILastRequestManager manager = manager;

    public void PrintToParticipants(string message)
    {
        prisoner.PrintToChat(message);
        guard.PrintToChat(message);
    }

    public abstract void Setup();
    public abstract void Execute();
    public abstract void OnEnd(LRResult result);
}