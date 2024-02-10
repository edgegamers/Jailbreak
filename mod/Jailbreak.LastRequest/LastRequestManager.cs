using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.LastRequest;

namespace Jailbreak.LastRequest;

public class LastRequestManager : IPluginBehavior, ILastRequestManager
{
    private BasePlugin _parent;

    public void Start(BasePlugin parent)
    {
        _parent = parent;
        _parent.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
    }

    public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        return HookResult.Continue;
    }

    public bool IsLREnabled { get; set; }
    public IList<AbstractLastRequest> ActiveLRs { get; } = new List<AbstractLastRequest>();

    public void InitiateLastRequest(AbstractLastRequest lastRequest)
    {
        lastRequest.Setup();
        ActiveLRs.Add(lastRequest);
    }
}