using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Extensions;

namespace Jailbreak.Debug.Subcommands;

public class DebugCenterHud(IServiceProvider services)
  : AbstractCommand(services) {
  public override void OnCommand(CCSPlayerController? executor,
    WrappedInfo info) {
    var rules = ServerExtensions.GetGameRules();
    if (rules == null) {
      info.ReplyToCommand("Failed to get GameRules.");
      return;
    }

    rules.GameRestart = !rules.GameRestart;

    if (info.ArgCount == 1)
      info.ReplyToCommand($"Set Center HUD to {rules.GameRestart}");

    executor?.PrintToCenterHtml("This is a test message", 100);
  }
}