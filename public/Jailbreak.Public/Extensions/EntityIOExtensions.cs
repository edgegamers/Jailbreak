using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Extensions;

public static class EntityIOExtensions {
  public static bool TryGetController(this CEntityInstance pawn,
    out CCSPlayerController? controller) {
    controller = null;

    if (!pawn.IsValid) return false;

    var index      = (int)pawn.Index;
    var playerPawn = Utilities.GetEntityFromIndex<CCSPlayerPawn>(index);

    if (!playerPawn.IsValid) return false;

    if (!playerPawn.OriginalController.IsValid) return false;

    controller = playerPawn.OriginalController.Value;

    return controller?.IsReal() == true;
  }
}