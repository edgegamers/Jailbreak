using GangsAPI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Gangs.WardenIconPerk;

public class WardenIconBootstrap {
  public WardenIconBootstrap(IServiceProvider collection) {
    new WardenIconCommand(collection).Start();
    collection.GetRequiredService<IPerkManager>()
     .Perks.Add(new WardenIconPerk(collection));
  }
}