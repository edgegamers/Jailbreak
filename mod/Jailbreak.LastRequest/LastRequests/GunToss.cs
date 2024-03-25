using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.LastRequest.LastRequests;

public class GunToss : AbstractLastRequest
{
    public GunToss(BasePlugin plugin, ILastRequestManager manager, CCSPlayerController prisoner,
        CCSPlayerController guard) : base(plugin, manager, prisoner, guard)
    {
    }

    public override LRType type => LRType.GunToss;

    public override void Setup()
    {
        // Strip weapons, teleport T to CT
        prisoner.RemoveWeapons();
        guard.RemoveWeapons();
        guard.Teleport(prisoner.Pawn.Value!.AbsOrigin!, prisoner.Pawn.Value.AbsRotation!, new Vector());
        state = LRState.Pending;

        plugin.AddTimer(3, Execute);
    }

    public override void Execute()
    {
        prisoner.GiveNamedItem("weapon_knife");
        guard.GiveNamedItem("weapon_knife");
        prisoner.GiveNamedItem("weapon_deagle");
        guard.GiveNamedItem("weapon_deagle");
        state = LRState.Active;
    }

    public override void OnEnd(LRResult result)
    {
        state = LRState.Completed;
    }
}