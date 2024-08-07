using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Extensions;

namespace Jailbreak.Debug.Subcommands;

public class DebugCenterHud(IServiceProvider services)
  : AbstractCommand(services) {
  public override void OnCommand(CCSPlayerController? executor,
    WrappedInfo info) {
    ServerExtensions.GetGameRules().GameRestart =
      !ServerExtensions.GetGameRules().GameRestart;

    if (info.ArgCount == 1)
      info.ReplyToCommand(
        $"Set Center HUD to {ServerExtensions.GetGameRules().GameRestart}");

    executor?.PrintToCenterHtml("This is a test message", 100);
  }
}