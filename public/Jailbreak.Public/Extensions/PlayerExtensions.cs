using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Extensions;

public static class PlayerExtensions
{
    public static CsTeam GetTeam(this CCSPlayerController controller)
    {
        return (CsTeam)controller.TeamNum;
    }

    public static bool IsReal(this CCSPlayerController? player)
    {
        //  Do nothing else before this:
        //  Verifies the handle points to an entity within the global entity list.
        if (player == null)
            return false;
        if (!player.IsValid)
            return false;

        if (player.Connected != PlayerConnectedState.PlayerConnected)
            return false;

        if (player.IsBot || player.IsHLTV)
            return false;

        return true;
    }

    public static void Teleport(this CCSPlayerController player, CCSPlayerController target)
    {
        if (!player.IsReal() || !target.IsReal())
            return;

        var playerPawn = player.Pawn.Value;
        if (playerPawn == null)
            return;

        var targetPawn = target.Pawn.Value;
        if (targetPawn == null)
            return;

        if (targetPawn is { AbsRotation: not null, AbsOrigin: not null })
            playerPawn.Teleport(targetPawn.AbsOrigin, targetPawn.AbsRotation, new Vector());
    }
}