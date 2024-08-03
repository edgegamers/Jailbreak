using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Zones;
using Jailbreak.Public.Utils;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.Zones;

public class RandomZoneGenerator(IZoneManager zoneManager, IZoneFactory factory)
  : IPluginBehavior {
  private BasePlugin plugin = null!;
  private IZone? cells;
  private IList<IZone>? manualSpawnPoints;
  private IList<IZone>? restrictedAreas;
  private Timer? timer;
  private string? currentMap;

  public void Start(BasePlugin basePlugin, bool hotreload) {
    plugin = basePlugin;

    startTimer();
  }

  void IDisposable.Dispose() { timer?.Kill(); }

  private void startTimer() {
    timer?.Kill();
    timer = plugin.AddTimer(1f, tick, TimerFlags.REPEAT);
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
    foreach (var player in PlayerUtil.GetAlive()) tick(player);
  }

  private void tick(CCSPlayerController player) {
    var pawn = player.PlayerPawn.Value;
    if (pawn == null) return;
    if (!pawn.OnGroundLastTick) return;
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

    var allSpawnPoints = (manualSpawnPoints ?? []).Concat(autoSpawnPoints ?? [])
     .ToList();

    if (allSpawnPoints.Count > 0) {
      dist = allSpawnPoints.Min(a => a.GetMinDistanceSquared(pos));
      if (dist <= DistanceZone.WIDTH_CELL * 2) return;
    }

    if (allSpawnPoints.Count > Server.MaxPlayers && autoSpawnPoints != null) {
      var nearestPoint = autoSpawnPoints
       .OrderBy(z => z.GetMinDistanceSquared(pos))
       .First();
      var currentScore = zoneScore(manualSpawnPoints);
      var newPoints    = new List<IZone>(allSpawnPoints);
      var zone         = factory.CreateZone([pos]);
      newPoints.Remove(nearestPoint);
      newPoints.Add(zone);

      var newScore = zoneScore(newPoints);

      Server.PrintToChatAll(
        $"Attempting to replace spawnpoint, old: {currentScore}, new: {newScore}");

      if (newScore <= currentScore) return;

      var map = Server.MapName;
      zone.Id = nearestPoint.Id;
      Server.NextFrameAsync(async () => {
        await zoneManager.DeleteZone(zone.Id, map);
        await zoneManager.PushZoneWithID(zone, ZoneType.SPAWN_AUTO, map);
      });

      return;
    }

    Server.PrintToChatAll(
      $"Creating new spawn point, not yet reached population limit. {allSpawnPoints.Count}/{Server.MaxPlayers}");

    var spawn = factory.CreateZone([pos]);
    autoSpawnPoints?.Add(spawn);
    Server.NextFrameAsync(async () => {
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

  private IList<IZone>? getManualSpawnPoints() {
    var zones = zoneManager.GetZones(ZoneType.SPAWN).GetAwaiter().GetResult();
    return zones;
  }

  private IList<IZone>? getAutoSpawnPoints() {
    var zones = zoneManager.GetZones(ZoneType.SPAWN_AUTO)
     .GetAwaiter()
     .GetResult();
    return zones;
  }

  private IZone? getCells() {
    var result = zoneManager.GetZones(ZoneType.CELL).GetAwaiter().GetResult();
    if (result.Count > 0) return new MultiZoneWrapper(result);

    var bounds = new DistanceZone(
      Utilities
       .FindAllEntitiesByDesignerName<SpawnPoint>("info_player_terrorist")
       .Where(s => s.AbsOrigin != null)
       .Select(s => s.AbsOrigin!)
       .ToList(), DistanceZone.WIDTH_CELL);
    return bounds;
  }

  private IZone getArmory() {
    var armory = zoneManager.GetZones(ZoneType.ARMORY).GetAwaiter().GetResult();
    if (armory.Count > 0) return new MultiZoneWrapper(armory);

    var bounds = new DistanceZone(
      Utilities
       .FindAllEntitiesByDesignerName<SpawnPoint>("info_player_terrorist")
       .Where(s => s.AbsOrigin != null)
       .Select(s => s.AbsOrigin!)
       .ToList(), DistanceZone.WIDTH_CELL);
    return bounds;
  }

  private IList<IZone>? getRestrictedAreas() {
    List<IZone> result = [];
    var tZones = zoneManager.GetZones(ZoneType.ZONE_LIMIT_T)
     .GetAwaiter()
     .GetResult();

    var ctZones = zoneManager.GetZones(ZoneType.ZONE_LIMIT_CT)
     .GetAwaiter()
     .GetResult();

    result.AddRange(tZones);
    result.AddRange(ctZones);
    result.Add(getArmory());
    return result;
  }
}