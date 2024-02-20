using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.LastRequest.LastRequests;

public class KnifeFight : AbstractLastRequest
{
    public KnifeFight(BasePlugin plugin, CCSPlayerController prisoner, CCSPlayerController guard) : base(plugin,
        prisoner, guard)
    {
    }

    public override void Setup()
    {
        // Strip weapons, teleport T to CT
        prisoner.RemoveWeapons();
        guard.RemoveWeapons();
        guard.Teleport(prisoner.Pawn.Value!.AbsOrigin!, prisoner.Pawn.Value.AbsRotation!, new Vector());
        prisoner.Pawn.Value.MoveType = MoveType_t.MOVETYPE_NONE;
        guard.Pawn.Value!.MoveType = MoveType_t.MOVETYPE_NONE;
        this.state = LRState.Pending;
        plugin.AddTimer(0.5f, () => guard.Pawn.Value.MoveType = MoveType_t.MOVETYPE_WALK);
        plugin.AddTimer(0.8f, () => guard.Pawn.Value.MoveType = MoveType_t.MOVETYPE_WALK);
        plugin.AddTimer(3, Execute);
    }

    public override void Execute()
    {
        prisoner.PrintToChat("Begin!");
        guard.PrintToChat("Begin!");
        prisoner.GiveNamedItem("weapon_knife");
        guard.GiveNamedItem("weapon_knife");
        this.state = LRState.Active;
    }

    public override void End(LRResult result)
    {
        this.state = LRState.Completed;
    }
}