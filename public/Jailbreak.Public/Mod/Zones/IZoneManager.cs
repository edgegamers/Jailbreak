using CounterStrikeSharp.API;
using Jailbreak.Public.Behaviors;

namespace Jailbreak.Public.Mod.Zones;

public interface IZoneManager : IPluginBehavior {
  Task LoadZones(string map);

  [Obsolete(
    "This method hides asynchroneous behavior, use the async version instead")]
  Task DeleteZone(int zoneId) {
    Server.NextFrame(() => {
      var map = Server.MapName;
      Server.NextFrameAsync(async () => { await DeleteZone(zoneId, map); });
    });
    return Task.CompletedTask;
  }

  Task DeleteZone(int zoneId, string map);

  Task<IList<IZone>> GetZones(string map, ZoneType type);

  async Task<IList<IZone>> GetZones(string map, params ZoneType[] type) {
    List<Task<IList<IZone>>> tasks = [];
    tasks.AddRange(type.Select(t => GetZones(t, map)));
    return (await Task.WhenAll(tasks)).SelectMany(x => x).ToList();
  }

  Task<IList<IZone>> GetZones(ZoneType type, string map) {
    return GetZones(map, type);
  }

  Task PushZoneWithID(IZone zone, ZoneType type, string map);
  Task PushZone(IZone zone, ZoneType type, string map);

  Task PushZone(IZone zone, ZoneType type) {
    return PushZone(zone, type, Server.MapName);
  }

  Task UpdateZone(IZone zone, ZoneType type, int id, string map);

  Task<Dictionary<ZoneType, IList<IZone>>> GetAllZones();
}