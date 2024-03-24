using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.LastRequest;

public class LastRequestManager : ILastRequestManager
{
    private BasePlugin _parent;
    private LastRequestConfig config;
    private ILastRequestMessages messages;
    private ILastRequestFactory factory;
    private IServiceProvider provider;

    public bool IsLREnabled { get; set; }
    public IList<AbstractLastRequest> ActiveLRs { get; } = new List<AbstractLastRequest>();

    public LastRequestManager(LastRequestConfig config, ILastRequestMessages messages, IServiceProvider provider)
    {
        this.config = config;
        this.messages = messages;
        this.provider = provider;
    }

    public void Start(BasePlugin parent)
    {
        this.factory = provider.GetRequiredService<ILastRequestFactory>();
        _parent = parent;
        _parent.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(OnTakeDamage, HookMode.Pre);
    }

    public void Dispose()
    {
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Unhook(OnTakeDamage, HookMode.Pre);
    }

    private HookResult OnTakeDamage(DynamicHook handle)
    {
        if (!IsLREnabled)
            return HookResult.Continue;
        CEntityInstance victim = handle.GetParam<CEntityInstance>(0);
        CTakeDamageInfo damage_info = handle.GetParam<CTakeDamageInfo>(1);

        CHandle<CBaseEntity> dealer = damage_info.Attacker;

        if (dealer.Value == null)
        {
            return HookResult.Continue;
        }

        // get player and attacker
        CCSPlayerController? player = new CCSPlayerController(new CBaseEntity(victim.Handle).Handle);
        CCSPlayerController? attacker = new CCSPlayerController(dealer.Value.Handle);

        if (!player.IsReal() || !attacker.IsReal())
            return HookResult.Continue;

        var playerLR = ((ILastRequestManager)this).GetActiveLR(player);
        var attackerLR = ((ILastRequestManager)this).GetActiveLR(attacker);

        if ((playerLR == null) != (attackerLR == null))
        {
            // One of them is in an LR
            attacker.PrintToChat("You or they are in LR, damage blocked.");
            damage_info.Damage = 0;
            return HookResult.Changed;
        }

        if (playerLR == null && attackerLR == null)
        {
            // Neither of them is in an LR
            return HookResult.Continue;
        }

        // Both of them are in LR
        // verify they're in same LR
        if (playerLR == null)
            return HookResult.Continue;

        if (playerLR.prisoner.Slot == attacker.Slot || playerLR.guard.Slot == attacker.Slot)
        {
            // Same LR, allow damage
            return HookResult.Changed;
        }

        attacker.PrintToChat("You are not in the same LR as them, damage blocked.");
        damage_info.Damage = 0;
        return HookResult.Continue;
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
        if (GetGameRules().WarmupPeriod)
            return HookResult.Continue;
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
        if (!player.IsReal() || GetGameRules().WarmupPeriod)
            return HookResult.Continue;

        if (IsLREnabled)
        {
            // Handle active LRs
            var activeLr = ((ILastRequestManager)this).GetActiveLR(player);
            if (activeLr != null && activeLr.state != LRState.Completed)
            {
                var isPrisoner = activeLr.prisoner.Slot == player.Slot;
                EndLastRequest(activeLr, isPrisoner ? LRResult.GuardWin : LRResult.PrisonerWin);
            }

            return HookResult.Continue;
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

            if (prisoner.Pawn.Value != null)
            {
                prisoner.Pawn.Value.Health = 100;
                prisoner.PlayerPawn.Value!.ArmorValue = 0;
                Utilities.SetStateChanged(prisoner.Pawn.Value, "CBaseEntity", "m_iHealth");
            }


            if (guard.Pawn.Value != null)
            {
                guard.Pawn.Value.Health = 100;
                guard.PlayerPawn.Value!.ArmorValue = 0;
                Utilities.SetStateChanged(guard.Pawn.Value, "CBaseEntity", "m_iHealth");
            }

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

    public static CCSGameRules GetGameRules()
    {
        return Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!;
    }
}