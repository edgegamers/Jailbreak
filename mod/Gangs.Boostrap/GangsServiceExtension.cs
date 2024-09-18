using CounterStrikeSharp.API.Core;
using Gangs.BombIconPerk;
using Jailbreak.Public;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Gangs.Boostrap;

public static class GangsServiceExtension {
  public static void AddGangs(this IServiceCollection services) {
    services.AddPluginBehavior<GangsInit>();
  }
}

public class GangsInit : IPluginBehavior {
  public void Start(BasePlugin basePlugin) {
    var services = API.Gangs?.Services;
    if (services == null) return;

    _ = new BombIconBootstrap(services);
  }
}