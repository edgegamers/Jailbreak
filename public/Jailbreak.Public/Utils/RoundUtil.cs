using CounterStrikeSharp.API;
using Jailbreak.Public.Extensions;

namespace Jailbreak.Public.Utils;

public static class RoundUtil {
  public static int GetTimeElapsed() {
    var gamerules = ServerExtensions.GetGameRules();
    if (gamerules == null) return 0;
    var freezeTime = gamerules.FreezeTime;
    return (int)(Server.CurrentTime - gamerules.RoundStartTime - freezeTime);
  }

  public static int GetTimeRemaining() {
    var gamerules = ServerExtensions.GetGameRules();
    if (gamerules == null) return 0;
    return gamerules.RoundTime - GetTimeElapsed();
  }

  public static void SetTimeRemaining(int seconds) {
    var gamerules = ServerExtensions.GetGameRules();
    if (gamerules == null) return;
    gamerules.RoundTime = GetTimeElapsed() + seconds;
    var proxy = ServerExtensions.GetGameRulesProxy();
    if (proxy == null) return;
    Utilities.SetStateChanged(proxy, "CCSGameRulesProxy", "m_pGameRules");
  }

  public static void AddTimeRemaining(int time) {
    var gamerules = ServerExtensions.GetGameRules();
    if (gamerules == null) return;
    gamerules.RoundTime += time;

    var proxy = ServerExtensions.GetGameRulesProxy();
    if (proxy == null) return;
    Utilities.SetStateChanged(proxy, "CCSGameRulesProxy", "m_pGameRules");
  }

  public static bool IsWarmup() {
    var rules = ServerExtensions.GetGameRules();
    return rules == null || rules.WarmupPeriod;
  }
}