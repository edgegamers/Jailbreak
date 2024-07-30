using CounterStrikeSharp.API;
using Jailbreak.Public.Behaviors;

namespace Jailbreak.Public.Mod.Zones;

public interface IZoneManager : IPluginBehavior {
  void DeleteZone(int zoneId) { DeleteZone(zoneId, Server.MapName); }
  void DeleteZone(int zoneId, string map);

  Task<IList<IZone>> GetZones(string map, ZoneType type);

  Task<IList<IZone>> GetZones(ZoneType type) {
    return GetZones(Server.MapName, type);
  }

  void PushZoneWithID(IZone zone, ZoneType type, string map);
  void PushZone(IZone zone, ZoneType type, string map);

  void PushZone(IZone zone, ZoneType type) {
    PushZone(zone, type, Server.MapName);
  }

  void UpdateZone(IZone zone, ZoneType type, int id);

  Task<IDictionary<ZoneType, IList<IZone>>> GetAllZones();
}