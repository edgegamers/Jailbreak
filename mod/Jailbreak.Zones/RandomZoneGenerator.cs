using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Zones;
using Jailbreak.Public.Utils;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.Zones;

public class RandomZoneGenerator(IZoneManager zoneManager, IZoneFactory factory)
  : IPluginBehavior {
  private Timer timer;
  private readonly BasePlugin? plugin;
  private IZone? cells;
  private IList<IZone>? restrictedAreas;
  private IList<IZone>? randomPoints;

  public void Start(BasePlugin basePlugin) {
    basePlugin.RegisterListener<Listeners.OnMapStart>(OnNewMap);
  }

  private void startTimer() {
    timer?.Kill();
    timer = plugin.AddTimer(63f, tick, TimerFlags.REPEAT);
  }

  private void OnNewMap(string mapname) {
    cells           = getCells();
    restrictedAreas = getRestrictedAreas();
    randomPoints    = getRandomPoints();
    startTimer();
  }

  private void tick() {
    foreach (var player in PlayerUtil.GetAlive()) tick(player);
  }

  private void tick(CCSPlayerController player) {
    var pawn = player.PlayerPawn.Value;
    if (pawn == null) return;
    if (!pawn.OnGroundLastTick) return;
    var pos = pawn.AbsOrigin;
    if (pos == null) return;
    pos = pos.Clone();
    if (cells != null && cells.IsInsideZone(pos)) return;
    if (restrictedAreas == null) return;
    var dist = restrictedAreas.Min(a => a.GetMinDistanceSquared(pos));
    if (dist <= DistanceZone.WIDTH_MEDIUM_ROOM) return;
    dist = restrictedAreas.Min(a => a.GetMinDistanceSquared(pos));
    if (dist <= DistanceZone.WIDTH_MEDIUM_ROOM) return;

    if (randomPoints?.Count > Server.MaxPlayers) {
      var nearestPoint = randomPoints.OrderBy(z => z.GetMinDistanceSquared(pos))
       .First();
      var currentScore = zoneScore(randomPoints);
      var newPoints    = new List<IZone>(randomPoints);
      newPoints.Remove(nearestPoint);
      if (!(zoneScore(newPoints) > currentScore)) return;

      randomPoints = newPoints;
      var map  = Server.MapName;
      var zone = factory.CreateZone([pos]);
      zone.Id = nearestPoint.Id;
      Server.NextFrameAsync(async () => {
        await zoneManager.DeleteZone(zone.Id, map);
        await zoneManager.PushZoneWithID(zone, ZoneType.SPAWN, map);
        randomPoints = getRandomPoints();
      });

      return;
    }

    var spawn = factory.CreateZone([pos]);
    zoneManager.PushZone(spawn, ZoneType.SPAWN);
  }

  private float zoneScore(IList<IZone>? zones) {
    var total = 0f;
    var count = 0;

    switch (zones?.Count) {
      case 1:
        return float.NegativeInfinity;
      case < 2:
        return 0f;
    }

    for (var i = 0; i < zones?.Count; i++) {
      for (var j = i + 1; j < zones.Count; j++) {
        total += zones[i]
         .GetCenterPoint()
         .DistanceSquared(zones[j].GetCenterPoint());
        count++;
      }
    }

    return total / count;
  }

  void IDisposable.Dispose() { timer.Kill(); }

  private IList<IZone>? getRandomPoints() {
    var zones = zoneManager.GetZones(ZoneType.SPAWN).GetAwaiter().GetResult();
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