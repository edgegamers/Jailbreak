using CounterStrikeSharp.API.Core;

namespace Jailbreak.Debug.Subcommands;

public class DebugCSay(IServiceProvider services) : AbstractCommand(services) {
  public override void OnCommand(CCSPlayerController? executor,
    WrappedInfo info) {
    if (executor == null) return;
    
    executor.PrintToCenterHtml(info.ArgString);
  }
}