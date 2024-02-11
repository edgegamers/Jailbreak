using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastRequest;

namespace Jailbreak.LastRequest;

public class LastRequestManager : IPluginBehavior, ILastRequestManager
{
    private BasePlugin _parent;
    private LastRequestConfig config;
    private ILastRequestMessages messages;

    public LastRequestManager(LastRequestConfig config, ILastRequestMessages messages)
    {
        this.config = config;
        this.messages = messages;
    }

    public void Start(BasePlugin parent)
    {
        _parent = parent;
        _parent.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
    }

    [GameEventHandler(HookMode.Post)]
    public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!player.IsReal())
            return HookResult.Continue;

        if (player.GetTeam() != CsTeam.Terrorist)
            return HookResult.Continue;

        int remainingPrisoners = Utilities.GetPlayers().Count(CountsToLR);
        if (remainingPrisoners > config.PrisonersToActiveLR)
            return HookResult.Continue;

        IsLREnabled = true;
        messages.LastRequestEnabled().ToAllChat();
        return HookResult.Continue;
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

    public void InitiateLastRequest(AbstractLastRequest lastRequest)
    {
        lastRequest.Setup();
        ActiveLRs.Add(lastRequest);
    }
}