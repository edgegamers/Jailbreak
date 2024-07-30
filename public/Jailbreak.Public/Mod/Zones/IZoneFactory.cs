using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Behaviors;

namespace Jailbreak.Public.Mod.Zones;

public interface IZoneFactory : IPluginBehavior {
  IZone CreateZone(IEnumerable<Vector> origins);
}