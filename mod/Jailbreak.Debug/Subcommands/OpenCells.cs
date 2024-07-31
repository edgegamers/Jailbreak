using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Utils;

namespace Jailbreak.Debug.Subcommands;

public class OpenCells(IServiceProvider services) : AbstractCommand(services) {
  public override void OnCommand(CCSPlayerController? executor,
    WrappedInfo info) {
    MapUtil.OpenCells();
  }
}