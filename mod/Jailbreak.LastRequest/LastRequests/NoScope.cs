using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.LastRequest.LastRequests;

public class NoScope : PvPDamageRequest
{
    public NoScope(BasePlugin plugin, CCSPlayerController prisoner, CCSPlayerController guard) : base(plugin,
        prisoner, guard)
    {
    }

    public override LRType type => LRType.NoScope;

    public override void Setup()
    {
        ((PvPDamageRequest)this).Setup();

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
            player.PlayerPawn.Value!.WeaponServices!.ActiveWeapon.Value!.NextSecondaryAttackTick = Server.TickCount + 500;
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
    }
}