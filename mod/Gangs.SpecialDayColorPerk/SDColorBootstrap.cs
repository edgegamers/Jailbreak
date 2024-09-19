using GangsAPI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Gangs.SpecialDayColorPerk;

public class SDColorBootstrap {
  public SDColorBootstrap(IServiceProvider collection) {
    new SDColorCommand(collection).Start();
    collection.GetRequiredService<IPerkManager>()
     .Perks.Add(new SDColorPerk(collection));
  }
}