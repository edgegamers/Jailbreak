using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.LastRequest.LastRequests;

public class NoScope(
    BasePlugin plugin,
    ILastRequestManager manager,
    CCSPlayerController prisoner,
    CCSPlayerController guard)
    : WeaponizedRequest(plugin, manager,
        prisoner, guard)
{
    public override LRType type => LRType.NoScope;

    public override void Setup()
    {
        base.Setup();

        plugin.RegisterListener<Listeners.OnTick>(OnTick);
    }

    private void OnTick()
    {
        if (this.state != LRState.Active) return;

        if (prisoner.PlayerPawn.Value == null || guard.PlayerPawn.Value == null) return;
        DisableScope(prisoner);
        DisableScope(guard);
    }

    private void DisableScope(CCSPlayerController player)
    {
        if (!player.IsReal())
            return;
        try
        {
            player.PlayerPawn.Value!.WeaponServices!.ActiveWeapon.Value!.NextSecondaryAttackTick =
                Server.TickCount + 500;
        }
        catch (NullReferenceException e)
        {
            Console.WriteLine(e);
        }
    }

    public override void Execute()
    {
        PrintToParticipants("Go!");
        prisoner.GiveNamedItem("weapon_ssg08");
        guard.GiveNamedItem("weapon_ssg08");
        this.state = LRState.Active;

        plugin.AddTimer(30, () =>
        {
            if (state != LRState.Active) return;
            prisoner.GiveNamedItem("weapon_knife");
            guard.GiveNamedItem("weapon_knife");
        }, TimerFlags.STOP_ON_MAPCHANGE);

        plugin.AddTimer(60, () =>
        {
            if (state != LRState.Active) return;

            manager.EndLastRequest(this, guard.Health > prisoner.Health ? LRResult.GuardWin : LRResult.PrisonerWin);
        }, TimerFlags.STOP_ON_MAPCHANGE);
    }

    public override void OnEnd(LRResult result)
    {
        plugin.RemoveListener("OnTick", OnTick);
    }
}