using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Mod.Zones;

public interface IZoneCreator {
  void BeginCreation();
  void AddPoint(Vector point);
  void DeletePoint(Vector point);
  IZone Build(IZoneFactory factory);
  void Dispose() { }
}