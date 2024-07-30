using CounterStrikeSharp.API;
using Jailbreak.Public.Behaviors;

namespace Jailbreak.SpecialDay;

public interface IZoneManager : IPluginBehavior {
  void LoadZones(string map);
  Task<IList<IZone>> GetZones(string map, ZoneType type);

  Task<IList<IZone>> GetZones(ZoneType type) {
    return GetZones(Server.MapName, type);
  }

  Task<IDictionary<ZoneType, IList<IZone>>> GetAllZones(string map);
}