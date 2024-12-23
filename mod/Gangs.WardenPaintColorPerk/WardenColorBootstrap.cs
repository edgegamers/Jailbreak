using GangsAPI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace WardenPaintColorPerk;

public class WardenColorBootstrap {
  public WardenColorBootstrap(IServiceProvider collection) {
    new WardenColorCommand(collection).Start();
    collection.GetRequiredService<IPerkManager>()
     .Perks.Add(new WardenPaintColorPerk(collection));
  }
}