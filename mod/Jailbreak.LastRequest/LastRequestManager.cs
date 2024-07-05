using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.SpecialDays;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;
using Jailbreak.Public.Mod.Damage;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.LastRequest;

public class LastRequestManager(
    LastRequestConfig _config,
    ILastRequestMessages _messages,
    IServiceProvider _provider,
    ISpecialDayHandler sdHandler
    )
    : ILastRequestManager, IBlockUserDamage
{
    private ISpecialDayHandler _sdHandler;
    private BasePlugin _parent;
    private ILastRequestFactory _factory;

    public bool IsLREnabled { get; set; }
    public IList<AbstractLastRequest> ActiveLRs { get; } = new List<AbstractLastRequest>();

    public void Start(BasePlugin parent)
    {
        _factory = _provider.GetRequiredService<ILastRequestFactory>();
        _parent = parent;
        _sdHandler = sdHandler;
    }

    [GameEventHandler(HookMode.Pre)]
    public HookResult OnTakeDamage(EventPlayerHurt @event, GameEventInfo info)
    {
        IBlockUserDamage damageHandler = this;
        return damageHandler.BlockUserDamage(@event, info);
    }

    public bool ShouldBlockDamage(CCSPlayerController player, CCSPlayerController? attacker, EventPlayerHurt @event)
    {
        if (!IsLREnabled)
            return false;

        if (attacker == null || !attacker.IsReal())
        {
            return false;
        }

        var playerLR = ((ILastRequestManager)this).GetActiveLR(player);
        var attackerLR = ((ILastRequestManager)this).GetActiveLR(attacker);

        if (playerLR == null && attackerLR == null)
        {
            // Neither of them is in an LR
            return false;
        }

        if ((playerLR == null) != (attackerLR == null))
        {
            // One of them is in an LR
            _messages.DamageBlockedInsideLastRequest.ToPlayerCenter(attacker);
            return true;
        }

        // Both of them are in LR
        // verify they're in same LR
        if (playerLR == null)
            return false;

        if (playerLR.prisoner.Equals(attacker) || playerLR.guard.Equals(attacker))
        {
            // Same LR, allow damage
            return false;
        }
        _messages.DamageBlockedNotInSameLR.ToPlayerCenter(attacker);
        return true;
    }

    [GameEventHandler]
    public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        if (IsLREnabled)
            foreach (var lr in ActiveLRs)
                EndLastRequest(lr, LRResult.TimedOut);

        IsLREnabled = false;
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        foreach (var player in Utilities.GetPlayers())
        {
            MenuManager.CloseActiveMenu(player);
        }
        if (ServerExtensions.GetGameRules().WarmupPeriod)
            return HookResult.Continue;
        if (CountAlivePrisoners() > _config.PrisonersToActiveLR)
        {
            this.IsLREnabled = false;
            return HookResult.Continue;
        }

        this.IsLREnabled = true;
        _messages.LastRequestEnabled().ToAllChat();
        return HookResult.Continue;
    }

    [GameEventHandler(HookMode.Post)]
    public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!player.IsReal() || ServerExtensions.GetGameRules().WarmupPeriod)
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

        if (CountAlivePrisoners() - 1 > _config.PrisonersToActiveLR)
            return HookResult.Continue;

        EnableLR();
        _messages.LastRequestEnabled().ToAllChat();
        return HookResult.Continue;
    }

    [GameEventHandler(HookMode.Post)]
    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var player = @event.Userid;

        if (player == null) return HookResult.Continue;
        
        if (!player.IsReal() || ServerExtensions.GetGameRules().WarmupPeriod)
            return HookResult.Continue;
        
        if (IsLREnabled)
            return HookResult.Continue;

        var activeLr = ((ILastRequestManager)this).GetActiveLR(player);

        if (activeLr != null || activeLr.state != LRState.Active)
        {
            EndLastRequest(activeLr, player.Team == CsTeam.Terrorist ? LRResult.GuardWin : LRResult.PrisonerWin);
            return HookResult.Continue;
        }
        
        if (player.GetTeam() != CsTeam.Terrorist)
            return HookResult.Continue;
        if (CountAlivePrisoners() > _config.PrisonersToActiveLR)
            return HookResult.Continue;

        EnableLR();
        _messages.LastRequestEnabled().ToAllChat();
        return HookResult.Continue;
    }

    public void DisableLR()
    {
        IsLREnabled = false;
    }

    public void EnableLR()
    {
        if (_sdHandler.IsSpecialDayActive()) return;
        IsLREnabled = true;
        SetRoundTime(60);

        foreach (var player in Utilities.GetPlayers())
        {
            if (!player.IsReal() || player.Team != CsTeam.Terrorist || !player.PawnIsAlive)
                continue;
            player.ExecuteClientCommandFromServer("css_lr");
        }
    }

    private int GetCurrentRoundTime()
    {
        var gamerules = ServerExtensions.GetGameRules();
        var timeleft = (gamerules.RoundStartTime + gamerules.RoundTime) - Server.CurrentTime;
        return (int)timeleft;
    }

    private int GetCurrentTimeElapsed()
    {
        var gamerules = ServerExtensions.GetGameRules();
        var freezeTime = gamerules.FreezeTime;
        return (int)((Server.CurrentTime - gamerules.RoundStartTime) - freezeTime);
    }

    private void SetRoundTime(int time)
    {
        var gamerules = ServerExtensions.GetGameRules();
        gamerules.RoundTime = (int)GetCurrentTimeElapsed() + time;

        Utilities.SetStateChanged(ServerExtensions.GetGameRulesProxy(), "CCSGameRulesProxy", "m_pGameRules");
    }

    private void AddRoundTime(int time)
    {
        var gamerules = ServerExtensions.GetGameRules();
        gamerules.RoundTime += time;

        Utilities.SetStateChanged(ServerExtensions.GetGameRulesProxy(), "CCSGameRulesProxy", "m_pGameRules");
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
            var lr = _factory.CreateLastRequest(prisoner, guard, type);
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

            _messages.InformLastRequest(lr).ToAllChat();
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
        {
            AddRoundTime(30);
            _messages.LastRequestDecided(lr, result).ToAllChat();
        }

        lr.OnEnd(result);
        ActiveLRs.Remove(lr);
        return true;
    }
}
