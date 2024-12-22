using GangsAPI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Gangs.LastRequestColorPerk;

public class LRColorBootstrap {
  public LRColorBootstrap(IServiceProvider collection) {
    new LRColorCommand(collection).Start();
    collection.GetRequiredService<IPerkManager>()
     .Perks.Add(new LRColorPerk(collection));
  }
}