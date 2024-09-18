using CounterStrikeSharp.API.Core;
using GangsAPI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Gangs.BombIconPerk;

public class BombIconBootstrap {
  public BombIconBootstrap(IServiceProvider collection, BasePlugin plugin) {
    new BombIconCommand(collection).Start();
    collection.GetRequiredService<IPerkManager>()
     .Perks.Add(new BombPerk(collection));
  }
}