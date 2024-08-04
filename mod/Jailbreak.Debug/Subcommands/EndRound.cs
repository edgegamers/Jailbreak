using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Utils;

namespace Jailbreak.Debug.Subcommands;

public class EndRound(IServiceProvider services) : AbstractCommand(services) {
  public override void OnCommand(CCSPlayerController? executor,
    WrappedInfo info) {
    Server.ExecuteCommand("mp_ignore_round_win_conditions 0");
    RoundUtil.SetTimeRemaining(0);
  }
}