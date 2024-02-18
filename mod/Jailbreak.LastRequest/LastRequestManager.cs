using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.LastRequest;

public class LastRequestManager : IPluginBehavior, ILastRequestManager
{
    private BasePlugin _parent;
    private LastRequestConfig config;
    private ILastRequestMessages messages;
    private ILastRequestFactory factory;

    public LastRequestManager(LastRequestConfig config, ILastRequestMessages messages, ILastRequestFactory factory)
    {
        this.config = config;
        this.messages = messages;
        this.factory = factory;
    }

    public void Start(BasePlugin parent)
    {
        _parent = parent;
        _parent.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
    }

    [GameEventHandler]
    public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        IsLREnabled = false;
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        if (CountAlivePrisoners() > config.PrisonersToActiveLR)
            return HookResult.Continue;
        this.IsLREnabled = true;
        return HookResult.Continue;
    }

    [GameEventHandler(HookMode.Post)]
    public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!player.IsReal())
            return HookResult.Continue;

        if (player.GetTeam() != CsTeam.Terrorist)
            return HookResult.Continue;

        if (CountAlivePrisoners() > config.PrisonersToActiveLR)
            return HookResult.Continue;

        IsLREnabled = true;
        messages.LastRequestEnabled().ToAllChat();
        return HookResult.Continue;
    }

    private int CountAlivePrisoners()
    {
        return Utilities.GetPlayers().Count(CountsToLR);
    }

    private bool CountsToLR(CCSPlayerController player)
    {
        if (!player.IsReal())
            return false;
        if (!player.PawnIsAlive)
            return false;
        return player.GetTeam() == CsTeam.Terrorist;
    }

    public bool IsLREnabled { get; set; }
    public IList<AbstractLastRequest> ActiveLRs { get; } = new List<AbstractLastRequest>();

    public void InitiateLastRequest(CCSPlayerController prisoner, CCSPlayerController guard, LRType type)
    {
        AbstractLastRequest lr = factory.CreateLastRequest(prisoner, guard, type);
        lr.Setup();
        ActiveLRs.Add(lr);
    }
}