using GangsAPI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Gangs.SpecialIconPerk;

public class SpecialIconBootstrap {
  public SpecialIconBootstrap(IServiceProvider collection) {
    new SpecialIconCommand(collection).Start();
    collection.GetRequiredService<IPerkManager>()
     .Perks.Add(new SpecialIconPerk(collection));
  }
}