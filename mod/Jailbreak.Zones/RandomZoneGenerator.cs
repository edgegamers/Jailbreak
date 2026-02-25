using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.Zones;
using Jailbreak.Public.Utils;
using Microsoft.Extensions.DependencyInjection;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.Zones;

public class RandomZoneGenerator(IZoneManager zoneManager, IZoneFactory factory,
  IServiceProvider provider) : IPluginBehavior {
  private IZone? cells;
  private string? currentMap;

  private List<IZone> manualSpawnPoints = new();
  private List<IZone> restrictedAreas = new();
  private List<IZone> autoSpawnPoints = new();

  private BasePlugin plugin = null!;
  private Timer? timer;

  public void Start(BasePlugin basePlugin, bool hotreload) {
    plugin = basePlugin;
    reload()
    startTimer();
  }

  void IDisposable.Dispose() { timer?.Kill(); }

  private void startTimer() {
    timer?.Kill();
    timer = plugin.AddTimer(63f, tick, TimerFlags.REPEAT);
  }

  private void reload() {
    var map = Server.MapName;
    
    Task.Run(async () => {
        var cellsTask = getCellsAsync(map);
        var manualTask = zoneManager.GetZones(map, ZoneType.SPAWN);
        var autoTask = zoneManager.GetZones(map, ZoneType.SPAWN_AUTO);
        var restrictedTask = getRestrictedAreasAsync(map);

        await Task.WhenAll(cellsTask, manualTask, autoTask, restrictedTask);
        Server.NextFrame(() => {
            cells = await cellsTask;
            manualSpawnPoints = (await manualTask).ToList();
            autoSpawnPoints = (await autoTask).ToList();
            restrictedAreas = (await restrictedTask).ToList();
            currentMap = map;
            
            if (manualSpawnPoints.Count > Server.MaxPlayers) {
                timer?.Kill();
            } else {
                startTimer();
            }
        });
    });
  }

  private void tick() {
    if (currentMap != Server.MapName) reload();
    currentMap = Server.MapName;

    var sdManager = provider.GetService<ISpecialDayManager>();
    if (sdManager is { IsSDRunning: true }) return;

    var autoSnapshot = new List<IZone>(autoSpawnPoints);
    var manualSnapshot = new List<IZone>(manualSpawnPoints);
    var restrictedSnapshot = new List<IZone>(restrictedAreas);

    foreach (var player in PlayerUtil.GetAlive()) {
        tick(player, autoSnapshot, manualSnapshot, restrictedSnapshot);
    }
  }

  private void tick(CCSPlayerController player, List<IZone> auto, List<IZone> manual, List<IZone> restricted) {
    var pawn = player.PlayerPawn.Value;
    if (pawn == null) return;
    if (!pawn.OnGroundLastTick || !pawn.TakesDamage) return;

    var pos = pawn.AbsOrigin;
    if (pos == null) return;
    pos = pos.Clone();

    float dist;
    
    for (int i = 0; i < 10; i++) {
        Task.Run(async () => {
            var temp = factory.CreateZone([pawn.AbsOrigin!]);
            await zoneManager.PushZone(temp, ZoneType.SPAWN_AUTO, Server.MapName);
        });
    }

    if (cells != null) {
      if (cells.IsInsideZone(pos)) return;
      dist = cells.GetMinDistanceSquared(pos);
      if (dist <= DistanceZone.WIDTH_CELL) return;
    }

    if (restrictedAreas is { Count: > 0 }) {
      dist = restrictedAreas.Min(a => a.GetMinDistanceSquared(pos));
      if (dist <= DistanceZone.WIDTH_MEDIUM_ROOM) return;
    }

    if (maunal != null
      && maunal.Count >= Server.MaxPlayers)
      return;

    var allSpawnPoints = (manual ?? []).Concat(auto)
     .ToList();

    if (allSpawnPoints.Count > 0) {
      dist = allSpawnPoints.Min(a => a.GetMinDistanceSquared(pos));
      if (dist <= DistanceZone.WIDTH_MEDIUM_ROOM) return;
    }

    if (allSpawnPoints.Count > Server.MaxPlayers) {
      var nearestPoint = autoSpawnPoints
       .OrderBy(z => z.GetMinDistanceSquared(pos))
       .FirstOrDefault();
      if (nearestPoint == null) return;

      var currentScore = zoneScore(maunal);
      var newPoints    = new List<IZone>(allSpawnPoints);
      var zone         = factory.CreateZone([pos]);
      newPoints.Remove(nearestPoint);
      newPoints.Add(zone);

      var newScore = zoneScore(newPoints);
      if (newScore <= currentScore) return;

      zone.Id = nearestPoint.Id;

      auto.Remove(nearestPoint);
      auto.Add(zone);
      autoSpawnPoints = new List<IZone>(auto); 

      _ = zoneManager.DeleteZone(zone.Id, currentMap!);
      _ = zoneManager.PushZoneWithID(zone, ZoneType.SPAWN_AUTO, currentMap!);
      return;
    }

    var spawn = factory.CreateZone([pos]);
    autoSpawnPoints.Add(spawn);
    _ = zoneManager.PushZone(spawn, ZoneType.SPAWN_AUTO, currentMap!);
  }

  private float zoneScore(IList<IZone>? zones) {
    // We want our spawnpoints to be as far away from each other as possible,
    // and as equally distributed as possible.

    if (zones == null || zones.Count == 0) return 0;

    var total = 0f;
    foreach (var zone in zones) {
      var min = zones.Min(z => z.GetMinDistanceSquared(zone.GetCenterPoint()));
      total += min;
    }

    return total;
  }

  private IList<IZone> getManualSpawnPoints() {
    var zones = zoneManager.GetZones(Server.MapName, ZoneType.SPAWN)
     .GetAwaiter()
     .GetResult();
    return zones;
  }

  private IList<IZone> getAutoSpawnPoints() {
    var zones = zoneManager.GetZones(Server.MapName, ZoneType.SPAWN_AUTO)
     .GetAwaiter()
     .GetResult();
    return zones;
  }

  private async Task<IZone?> getCellsAsync(string map) {
    var result = await zoneManager.GetZones(map, ZoneType.CELL);
    if (result.Count > 0) return new MultiZoneWrapper(result);

    var origins = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>("info_player_terrorist")
        .Where(s => s.AbsOrigin != null)
        .Select(s => s.AbsOrigin!.Clone())
        .ToList();

    return new DistanceZone(origins, DistanceZone.WIDTH_CELL);
  }

  private Task<List<IZone>> getRestrictedAreasAsync(string map) {
    List<IZone> result = [];
    foreach (var zone in ZoneTypeExtensions.DoNotTeleports()) {
      result.AddRange(await zoneManager.GetZones(map, zoneType));
    }

    var armory = zoneManager.GetZones(Server.MapName, ZoneType.ARMORY)
     .GetAwaiter()
     .GetResult();
    if (armory.Count == 0) {
      var bounds = new DistanceZone(
        Utilities
         .FindAllEntitiesByDesignerName<
            SpawnPoint>("info_player_counterterrorist")
         .Where(s => s.AbsOrigin != null)
         .Select(s => s.AbsOrigin!)
         .ToList(), DistanceZone.WIDTH_MEDIUM_ROOM);
      result.Add(bounds);
    }

    return result;
  }
}