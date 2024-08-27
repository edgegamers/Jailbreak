using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Utils;

public static class TeamUtil {
  public static CsTeam? FromString(string team) {
    return team.ToLower() switch {
      "0" or "n" or "none" => CsTeam.None,
      "1" or "s" or "spec" or "spectator" or "spectators" or "specs" => CsTeam
       .Spectator,
      "2" or "t" or "ts" or "terror" or "terrorist" or "terrorists"
        or "prisoner" or "prisoners" => CsTeam.Terrorist,
      "3" or "ct" or "cts" or "counter" or "counterterrorist"
        or "counterterrorists" or "guard"
        or "guards" => CsTeam.CounterTerrorist,
      _ => null
    };
  }
}