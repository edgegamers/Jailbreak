using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Behaviors;

namespace Jailbreak.SpecialDay;

public interface IZoneFactory : IPluginBehavior {
  IZone CreateZone(IEnumerable<Vector> origins);
}