using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Extensions;

public static class ServerExtensions {
  /// <summary>
  ///   Get the current CCSGameRules for the server
  /// </summary>
  /// <returns></returns>
  public static CCSGameRules? GetGameRules() {
    //	From killstr3ak
    return Utilities
     .FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules")
     .First()
     .GameRules;
  }

  public static CCSGameRulesProxy? GetGameRulesProxy() {
    //	From killstr3ak
    return Utilities
     .FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules")
     .FirstOrDefault();
  }
}