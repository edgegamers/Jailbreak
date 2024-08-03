using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Utils;

namespace Jailbreak.Debug.Subcommands;

public class TestNearOpen(IServiceProvider services)
  : AbstractCommand(services) {
  public override void OnCommand(CCSPlayerController? executor,
    WrappedInfo info) {
    if (executor == null) return;
    var pos = executor.Pawn.Value?.AbsOrigin;
    if (pos == null) return;
    MapUtil.OpenCells(Sensitivity.ANY, pos);
    info.ReplyToCommand(
      "Attempted to open cells using the button nearest to you.");
  }
}