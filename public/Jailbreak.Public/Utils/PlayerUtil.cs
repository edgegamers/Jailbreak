using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Utils;

public static class PlayerUtil {
  public static IEnumerable<CCSPlayerController> FromTeam(CsTeam team,
    bool alive = true) {
    return Utilities.GetPlayers()
     .Where(p => p.Team == team && (!alive || p.PawnIsAlive));
  }

  public static CCSPlayerController? GetRandomFromTeam(CsTeam team,
    bool alive = true) {
    return Utilities.GetPlayers()
     .FirstOrDefault(p => p.Team == team && (!alive || p.PawnIsAlive));
  }

  public static IEnumerable<CCSPlayerController> GetAlive() {
    return Utilities.GetPlayers().Where(p => p.PawnIsAlive);
  }
}