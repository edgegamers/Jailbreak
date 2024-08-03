using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Mod.Zones;
using Jailbreak.Public.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Debug.Subcommands;

public class OpenCells(IServiceProvider services) : AbstractCommand(services) {
  public override void OnCommand(CCSPlayerController? executor,
    WrappedInfo info) {
    MapUtil.OpenCells(Services.GetRequiredService<IZoneManager>());
  }
}