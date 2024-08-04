using CounterStrikeSharp.API;
using Jailbreak.Public.Behaviors;

namespace Jailbreak.Public.Mod.Zones;

public interface IZoneManager : IPluginBehavior {
  Task LoadZones(string map);
  Task DeleteZone(int zoneId) { return DeleteZone(zoneId, Server.MapName); }
  Task DeleteZone(int zoneId, string map);

  Task<IList<IZone>> GetZones(string map, ZoneType type);

  async Task<IList<IZone>> GetZones(params ZoneType[] type) {
    List<Task<IList<IZone>>> tasks = [];
    tasks.AddRange(type.Select(GetZones));
    return (await Task.WhenAll(tasks)).SelectMany(x => x).ToList();
  }

  Task<IList<IZone>> GetZones(ZoneType type) {
    return GetZones(Server.MapName, type);
  }

  Task PushZoneWithID(IZone zone, ZoneType type, string map);
  Task PushZone(IZone zone, ZoneType type, string map);

  Task PushZone(IZone zone, ZoneType type) {
    return PushZone(zone, type, Server.MapName);
  }

  Task UpdateZone(IZone zone, ZoneType type, int id);

  Task<Dictionary<ZoneType, IList<IZone>>> GetAllZones();
}