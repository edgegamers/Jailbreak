using Jailbreak.Public.Extensions;
using Jailbreak.SpecialDay;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Zones;

public static class ZoneServiceExtension {
  public static void AddJailbreakZones(this IServiceCollection service) {
    service.AddPluginBehavior<IZoneFactory, CHZZoneFactory>();
    service.AddPluginBehavior<IZoneManager, SqlZoneManager>();
  }
}