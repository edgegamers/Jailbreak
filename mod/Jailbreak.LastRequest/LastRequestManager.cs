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

public class LastRequestManager : ILastRequestManager
{
    private BasePlugin _parent;
    private LastRequestConfig config;
    private ILastRequestMessages messages;
    private ILastRequestFactory factory;

    public bool IsLREnabled { get; set; }
    public IList<AbstractLastRequest> ActiveLRs { get; } = new List<AbstractLastRequest>();

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
        messages.LastRequestEnabled().ToAllChat();
        return HookResult.Continue;
    }

    [GameEventHandler(HookMode.Post)]
    public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!player.IsReal())
            return HookResult.Continue;

        if (IsLREnabled && ((ILastRequestManager)this).IsInLR(player))
        {
            // Handle active LRs
            var activeLr = ((ILastRequestManager)this).GetActiveLR(player);
            if (activeLr != null)
            {
                var isPrisoner = activeLr.prisoner.Slot == player.Slot;
                EndLastRequest(activeLr, isPrisoner ? LRResult.GuardWin : LRResult.PrisonerWin);
                return HookResult.Continue;
            }
        }

        if (player.GetTeam() != CsTeam.Terrorist)
            return HookResult.Continue;

        if (CountAlivePrisoners() - 1 > config.PrisonersToActiveLR)
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

    public bool InitiateLastRequest(CCSPlayerController prisoner, CCSPlayerController guard, LRType type)
    {
        try
        {
            var lr = factory.CreateLastRequest(prisoner, guard, type);
            lr.Setup();
            ActiveLRs.Add(lr);

            messages.InformLastRequest(lr).ToPlayerChat(prisoner);
            messages.InformLastRequest(lr).ToPlayerChat(guard);
            return true;
        }
        catch (ArgumentException e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public bool EndLastRequest(AbstractLastRequest lr, LRResult result)
    {
        if (result is LRResult.GuardWin or LRResult.PrisonerWin)
            messages.LastRequestDecided(lr, result).ToAllChat();
        lr.OnEnd(result);
        ActiveLRs.Remove(lr);
        return true;
    }
}