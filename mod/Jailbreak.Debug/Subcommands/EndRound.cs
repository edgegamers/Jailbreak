using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Utils;

namespace Jailbreak.Debug.Subcommands;

public class EndRound(IServiceProvider services) : AbstractCommand(services) {
  public override void OnCommand(CCSPlayerController? executor,
    WrappedInfo info) {
    RoundUtil.SetTimeRemaining(0);
  }
}