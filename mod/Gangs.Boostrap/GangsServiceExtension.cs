using CounterStrikeSharp.API.Core;
using Gangs.BombIconPerk;
using Gangs.CellsPerk;
using Gangs.LastRequestColorPerk;
using Gangs.SpecialDayColorPerk;
using Gangs.WardenIconPerk;
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
    _ = new SDColorBootstrap(services);
    _ = new CellsPerkBootstrap(services);
    _ = new LRColorBootstrap(services);
    _ = new WardenIconBootstrap(services);
  }
}