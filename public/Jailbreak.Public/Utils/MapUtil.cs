using System.Diagnostics.CodeAnalysis;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Zones;

namespace Jailbreak.Public.Utils;

[SuppressMessage("ReSharper",
  "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract")]
public static class MapUtil {
  private static Vector getCtSpawn() {
    return Utilities
     .FindAllEntitiesByDesignerName<SpawnPoint>("info_player_counterterrorist")
     .Where(s => s.AbsOrigin != null)
     .Select(s => s.AbsOrigin!)
     .First();
  }

  public static bool OpenCells(IZoneManager zoneManager) {
    var zones = zoneManager.GetZones(Server.MapName, ZoneType.CELL_BUTTON)
     .GetAwaiter()
     .GetResult();


    if (zones == null || zones.Count == 0)
      return OpenCells() <= Sensitivity.TARGET_CELL;

    return OpenCells(Sensitivity.ANY, zones.First().GetCenterPoint()) != null;
  }

  public static Sensitivity? OpenCells(
    Sensitivity sensitivity = Sensitivity.NAME_CELL_DOOR,
    Vector? source = null) {
    if (source == null) source = getCtSpawn();
    var allButtons = Utilities
     .FindAllEntitiesByDesignerName<CEntityInstance>("func_button")
     .ToHashSet();

    IDictionary<int, CBaseEntity> entityCache =
      new Dictionary<int, CBaseEntity>();

    foreach (var button in allButtons) {
      var ent = Utilities.GetEntityFromIndex<CBaseEntity>((int)button.Index);
      if (ent == null || !ent.IsValid) continue;
      entityCache[(int)button.Index] = ent;
    }

    while (true) {
      IList<CBaseEntity> entities = [];
      foreach (var button in allButtons
       .Select(cell => entityCache[(int)cell.Index])
       .Where(button => button != null && button.IsValid)
       .Where(button => IsCellButton(button, sensitivity)))
        entities.Add(button);

      switch (entities.Count) {
        case 1:
          PressButton(entities[0]);
          return sensitivity;
        case 0: {
          var lower = sensitivity.GetLower();
          if (lower == null) return lower;
          sensitivity = lower.Value;
          continue;
        }
      }

      var sorted = entities.ToList();
      sorted.Sort((a, b) => {
        var aDist = a.AbsOrigin!.DistanceSquared(source);
        var bDist = b.AbsOrigin!.DistanceSquared(source);
        return aDist.CompareTo(bDist);
      });

      PressButton(sorted[0]);
      break;
    }

    return sensitivity;
  }

  private static void PressButton(CBaseEntity entity) {
    entity.AcceptInput("Unlock",
      PlayerUtil.FromTeam(CsTeam.CounterTerrorist).FirstOrDefault());
    entity.AcceptInput("Press",
      PlayerUtil.FromTeam(CsTeam.CounterTerrorist).FirstOrDefault());
  }

  private static bool IsCellButton(CBaseEntity ent, Sensitivity sen) {
    if (!ent.IsValid) return false;
    var button = Utilities.GetEntityFromIndex<CBaseEntity>((int)ent.Index);
    if (button == null || button.Entity == null || !button.IsValid)
      return false;
    var name       = button.Entity.Name;
    var targetName = button.Target;

    switch (sen) {
      case Sensitivity.NAME_CELL_DOOR:
        return name != null
          && name.Contains("cell", StringComparison.CurrentCultureIgnoreCase)
          && name.Contains("door", StringComparison.CurrentCultureIgnoreCase);
      case Sensitivity.NAME_CELL:
        return name != null && name.Contains("cell",
          StringComparison.CurrentCultureIgnoreCase);
      case Sensitivity.TARGET_CELL_DOOR:
        return targetName != null
          && targetName.Contains("cell",
            StringComparison.CurrentCultureIgnoreCase)
          && targetName.Contains("door",
            StringComparison.CurrentCultureIgnoreCase);
      case Sensitivity.TARGET_CELL:
        return targetName != null && targetName.Contains("cell",
          StringComparison.CurrentCultureIgnoreCase);
      case Sensitivity.ANY_WITH_TARGET:
        return targetName != null;
      case Sensitivity.ANY:
        return true;
    }

    return false;
  }

  public static List<Vector> GetRandomSpawns(int count, IZoneManager? zones) {
    // Progressively get more lax with our "randomness quality"
    var result = GetRandomSpawns(zones, false, false, false);
    if (result.Count < count) result = GetRandomSpawns(zones, false, false);
    if (result.Count < count) result = GetRandomSpawns(zones, false);
    if (result.Count < count) result = GetRandomSpawns(zones);
    return result;
  }

  public static List<Vector> GetRandomSpawns(IZoneManager? zoneManager = null,
    bool includeSpawns = true, bool includeTps = true,
    bool includeAuto = true) {
    var result = new List<Vector>();

    if (includeTps) {
      var worldTp = Utilities
       .FindAllEntitiesByDesignerName<CInfoTeleportDestination>(
          "info_teleport_destination")
       .Where(s => s.AbsOrigin != null)
       .Select(s => s.AbsOrigin!);
      result.AddRange(worldTp);
    }

    if (includeSpawns) {
      var tSpawns = Utilities
       .FindAllEntitiesByDesignerName<SpawnPoint>("info_player_terrorist")
       .Where(s => s.AbsOrigin != null)
       .Select(s => s.AbsOrigin!);
      var ctSpawns = Utilities
       .FindAllEntitiesByDesignerName<
          SpawnPoint>("info_player_counterterrorist")
       .Where(s => s.AbsOrigin != null)
       .Select(s => s.AbsOrigin!);
      result.AddRange(tSpawns);
      result.AddRange(ctSpawns);
    }

    if (zoneManager != null) {
      if (includeAuto)
        result.AddRange(zoneManager
         .GetZones(Server.MapName, ZoneType.SPAWN_AUTO)
         .GetAwaiter()
         .GetResult()
         .Select(z => z.GetCenterPoint()));

      var zones = zoneManager.GetZones(Server.MapName, ZoneType.SPAWN)
       .GetAwaiter()
       .GetResult();
      result.AddRange(zones.Select(z => z.GetCenterPoint()));
    }

    result.Shuffle();
    return result;
  }
}

public enum Sensitivity {
  NAME_CELL_DOOR,
  NAME_CELL,
  TARGET_CELL_DOOR,
  TARGET_CELL,
  ANY_WITH_TARGET,
  ANY
}

public static class SensitivityExtensions {
  public static Sensitivity? GetLower(this Sensitivity sen) {
    return sen switch {
      Sensitivity.NAME_CELL_DOOR   => Sensitivity.NAME_CELL,
      Sensitivity.NAME_CELL        => Sensitivity.TARGET_CELL_DOOR,
      Sensitivity.TARGET_CELL_DOOR => Sensitivity.TARGET_CELL,
      Sensitivity.TARGET_CELL      => Sensitivity.ANY_WITH_TARGET,
      Sensitivity.ANY_WITH_TARGET  => Sensitivity.ANY,
      _                            => null
    };
  }
}