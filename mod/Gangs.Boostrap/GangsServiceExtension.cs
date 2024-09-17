using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Gangs.BombIconPerk;
using Jailbreak.Public;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Gangs.Boostrap;

public static class GangsServiceExtension {
  public static void AddGangs(this IServiceCollection services) {
    services.AddPluginBehavior<GangsInit>();
  }
}

public class GangsInit : IPluginBehavior {
  public void Start(BasePlugin basePlugin) {
    basePlugin.Logger.LogInformation("Timer fired");
    var services = API.Gangs?.Services;
    basePlugin.Logger.LogInformation(
      "Found services: {services} (is null: {null}) ({Gangs}) (null: {isNull})",
      services, services == null, API.Gangs, API.Gangs == null);
    if (services == null) return;

    _ = new BombIconBootstrap(services, basePlugin);
  }
}