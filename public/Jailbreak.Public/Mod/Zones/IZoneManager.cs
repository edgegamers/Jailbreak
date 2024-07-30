using CounterStrikeSharp.API;
using Jailbreak.Public.Behaviors;

namespace Jailbreak.Public.Mod.Zones;

public interface IZoneManager : IPluginBehavior {
  void LoadZones(string map);

  void DeleteZone(int zoneId) { DeleteZone(zoneId, Server.MapName); }
  void DeleteZone(int zoneId, string map);

  Task<IList<IZone>> GetZones(string map, ZoneType type);

  Task<IList<IZone>> GetZones(ZoneType type) {
    return GetZones(Server.MapName, type);
  }

  Task PushZone(IZone zone, ZoneType type, string map);

  Task PushZone(IZone zone, ZoneType type) {
    return PushZone(zone, type, Server.MapName);
  }

  Task<IDictionary<ZoneType, IList<IZone>>> GetAllZones(string map);
}