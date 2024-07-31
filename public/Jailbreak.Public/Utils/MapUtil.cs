using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Utils;

public static class MapUtil {
  public static void OpenCells() {
    foreach (var ent in
      Utilities.FindAllEntitiesByDesignerName<CEntityInstance>("func_button")) {
      if (!ent.IsValid) continue;
      var button = Utilities.GetEntityFromIndex<CBaseEntity>((int)ent.Index);
      if (button == null || button.Entity == null || !button.IsValid) continue;
      if (!button.Entity.Name.Contains("cell",
        StringComparison.CurrentCultureIgnoreCase))
        continue;
      Server.PrintToConsole(
        $"Found button: {ent} {ent.DesignerName} {button?.Entity?.Name}");
      ent.AcceptInput("Unlock");
      ent.AcceptInput("Press");
    }
  }
}