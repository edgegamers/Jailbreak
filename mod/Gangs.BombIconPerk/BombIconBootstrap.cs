using GangsAPI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Gangs.BombIconPerk;

public class BombIconBootstrap {
  public BombIconBootstrap(IServiceProvider collection) {
    new BombIconCommand(collection).Start();
    collection.GetRequiredService<IPerkManager>()
     .Perks.Add(new BombPerk(collection));
  }
}