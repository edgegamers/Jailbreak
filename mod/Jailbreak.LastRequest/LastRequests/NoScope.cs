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
        if (state != LRState.Active) return;

        if (!prisoner.IsReal() || !guard.IsReal())
            return;

        if (prisoner.PlayerPawn.Value == null || guard.PlayerPawn.Value == null) return;
        DisableScope(prisoner);
        DisableScope(guard);
    }

    private void DisableScope(CCSPlayerController player)
    {
        if (!player.IsReal())
            return;
        var pawn = player.PlayerPawn.Value;
        if (pawn == null || !pawn.IsValid)
            return;
        var weaponServices = pawn.WeaponServices;
        if (weaponServices == null)
            return;
        var activeWeapon = weaponServices.ActiveWeapon.Value;
        if (activeWeapon == null || !activeWeapon.IsValid)
            return;
        activeWeapon.NextSecondaryAttackTick = Server.TickCount + 500;
    }

    public override void Execute()
    {
        PrintToParticipants("Go!");
        prisoner.GiveNamedItem("weapon_ssg08");
        guard.GiveNamedItem("weapon_ssg08");
        state = LRState.Active;

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
        state = LRState.Completed;
        plugin.RemoveListener("OnTick", OnTick);

        if (result != LRResult.GuardWin && result != LRResult.PrisonerWin)
            return;

        var winner = result == LRResult.GuardWin ? guard : prisoner;

        winner.RemoveWeapons();
        winner.GiveNamedItem("weapon_knife");
        winner.GiveNamedItem("weapon_ak47");
    }
}