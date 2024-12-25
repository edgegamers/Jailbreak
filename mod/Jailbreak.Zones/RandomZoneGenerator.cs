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
  private IList<IZone>? manualSpawnPoints;
  private BasePlugin plugin = null!;
  private IList<IZone>? restrictedAreas;
  private Timer? timer;

  public void Start(BasePlugin basePlugin, bool hotreload) {
    plugin = basePlugin;

    startTimer();
  }

  void IDisposable.Dispose() { timer?.Kill(); }

  private void startTimer() {
    timer?.Kill();
    timer = plugin.AddTimer(63f, tick, TimerFlags.REPEAT);
  }

  private void reload() {
    cells             = getCells();
    restrictedAreas   = getRestrictedAreas();
    manualSpawnPoints = getManualSpawnPoints();

    if (manualSpawnPoints?.Count > Server.MaxPlayers) return;

    startTimer();
  }

  private void tick() {
    if (currentMap != Server.MapName) reload();
    currentMap = Server.MapName;

    var sdManager = provider.GetService<ISpecialDayManager>();
    if (sdManager is { IsSDRunning: true }) return;

    foreach (var player in PlayerUtil.GetAlive()) tick(player);
  }

  private void tick(CCSPlayerController player) {
    var pawn = player.PlayerPawn.Value;
    if (pawn == null) return;
    if (!pawn.OnGroundLastTick || !pawn.TakesDamage) return;
    var pos = pawn.AbsOrigin;
    if (pos == null) return;
    pos = pos.Clone();
    float dist;
    if (cells != null) {
      if (cells.IsInsideZone(pos)) return;
      dist = cells.GetMinDistanceSquared(pos);
      if (dist <= DistanceZone.WIDTH_CELL) return;
    }

    if (restrictedAreas is { Count: > 0 }) {
      dist = restrictedAreas.Min(a => a.GetMinDistanceSquared(pos));
      if (dist <= DistanceZone.WIDTH_MEDIUM_ROOM) return;
    }

    if (manualSpawnPoints != null
      && manualSpawnPoints.Count >= Server.MaxPlayers)
      return;

    var autoSpawnPoints = getAutoSpawnPoints();

    var allSpawnPoints = (manualSpawnPoints ?? []).Concat(autoSpawnPoints)
     .ToList();

    if (allSpawnPoints.Count > 0) {
      dist = allSpawnPoints.Min(a => a.GetMinDistanceSquared(pos));
      if (dist <= DistanceZone.WIDTH_MEDIUM_ROOM) return;
    }

    if (allSpawnPoints.Count > Server.MaxPlayers) {
      var nearestPoint = autoSpawnPoints
       .OrderBy(z => z.GetMinDistanceSquared(pos))
       .First();
      var currentScore = zoneScore(manualSpawnPoints);
      var newPoints    = new List<IZone>(allSpawnPoints);
      var zone         = factory.CreateZone([pos]);
      newPoints.Remove(nearestPoint);
      newPoints.Add(zone);

      var newScore = zoneScore(newPoints);

      if (newScore <= currentScore) return;

      var map = Server.MapName;
      zone.Id = nearestPoint.Id;
      // Server.NextFrameAsync(async () => {
      Task.Run(async () => {
        await zoneManager.DeleteZone(zone.Id, map);
        await zoneManager.PushZoneWithID(zone, ZoneType.SPAWN_AUTO, map);
      });

      return;
    }

    var spawn = factory.CreateZone([pos]);
    autoSpawnPoints.Add(spawn);
    // Server.NextFrameAsync(async () => {
    Task.Run(async () => {
      await zoneManager.PushZone(spawn, ZoneType.SPAWN_AUTO, currentMap!);
    });
  }

  private float zoneScore(IList<IZone>? zones) {
    // We want our spawnpoints to be as far away from each other as possible,
    // and as equally distributed as possible.

    if (zones == null) return 0;

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

  private IZone getCells() {
    var result = zoneManager.GetZones(Server.MapName, ZoneType.CELL)
     .GetAwaiter()
     .GetResult();
    if (result.Count > 0) return new MultiZoneWrapper(result);

    var bounds = new DistanceZone(
      Utilities
       .FindAllEntitiesByDesignerName<SpawnPoint>("info_player_terrorist")
       .Where(s => s.AbsOrigin != null)
       .Select(s => s.AbsOrigin!)
       .ToList(), DistanceZone.WIDTH_CELL);
    return bounds;
  }

  private IList<IZone> getRestrictedAreas() {
    List<IZone> result = [];
    foreach (var zone in ZoneTypeExtensions.DoNotTeleports()) {
      var zones = zoneManager.GetZones(Server.MapName, zone)
       .GetAwaiter()
       .GetResult();
      result.AddRange(zones);
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