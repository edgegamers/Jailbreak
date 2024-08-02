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
    var players = FromTeam(team, alive).Where(p => !p.IsBot).ToList();
    return players.Count == 0 ?
      null :
      players[new Random().Next(players.Count)];
  }

  public static IEnumerable<CCSPlayerController> GetAlive() {
    return Utilities.GetPlayers().Where(p => p.PawnIsAlive);
  }
}