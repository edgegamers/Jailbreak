using CounterStrikeSharp.API;
using Jailbreak.Public.Extensions;

namespace Jailbreak.Public.Utils;

public static class RoundUtil {
  public static int GetTimeElapsed() {
    var gamerules  = ServerExtensions.GetGameRules();
    var freezeTime = gamerules.FreezeTime;
    return (int)(Server.CurrentTime - gamerules.RoundStartTime - freezeTime);
  }

  public static int GetTimeRemaining() {
    var gamerules = ServerExtensions.GetGameRules();
    return gamerules.RoundTime - GetTimeElapsed();
  }

  public static void SetTimeRemaining(int seconds) {
    var gamerules = ServerExtensions.GetGameRules();
    gamerules.RoundTime = GetTimeElapsed() + seconds;

    Utilities.SetStateChanged(ServerExtensions.GetGameRulesProxy(),
      "CCSGameRulesProxy", "m_pGameRules");
  }

  public static void AddTimeRemaining(int time) {
    var gamerules = ServerExtensions.GetGameRules();
    gamerules.RoundTime += time;

    Utilities.SetStateChanged(ServerExtensions.GetGameRulesProxy(),
      "CCSGameRulesProxy", "m_pGameRules");
  }

  public static bool IsWarmup() {
    return ServerExtensions.GetGameRules().WarmupPeriod;
  }
}