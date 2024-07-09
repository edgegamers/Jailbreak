using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Extensions;

public static class ServerExtensions {
  public static void PrintToCenterAll(string message) {
    VirtualFunctions.ClientPrintAll(HudDestination.Center, message, 0, 0, 0, 0);
  }

  /// <summary>
  ///   Get the current CCSGameRules for the server
  /// </summary>
  /// <returns></returns>
  public static CCSGameRules GetGameRules() {
    //	From killstr3ak
    return Utilities
     .FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules")
     .First()
     .GameRules!;
  }

  public static CCSGameRulesProxy GetGameRulesProxy() {
    //	From killstr3ak
    return Utilities
     .FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules")
     .First()!;
  }
}