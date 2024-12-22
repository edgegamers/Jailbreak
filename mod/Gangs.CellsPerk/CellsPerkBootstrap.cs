using GangsAPI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Gangs.CellsPerk;

public class CellsPerkBootstrap {
  public CellsPerkBootstrap(IServiceProvider collection) {
    collection.GetRequiredService<IPerkManager>()
     .Perks.Add(new CellsPerk(collection));
  }
}