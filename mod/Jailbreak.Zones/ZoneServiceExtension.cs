using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Zones;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Zones;

public static class ZoneServiceExtension {
  public static void AddJailbreakZones(this IServiceCollection service) {
    service.AddPluginBehavior<IZoneFactory, BasicZoneFactory>();
    service.AddPluginBehavior<IZoneManager, SqlZoneManager>();
    service.AddPluginBehavior<RandomZoneGenerator>();
  }
}