using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Objects;
using Jailbreak.Formatting.Views;
using Jailbreak.Formatting.Views.Logging;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;

namespace Jailbreak.Logs;

public class LogsManager(ILogLocale locale, IRichPlayerTag richPlayerTag)
  : IPluginBehavior, IRichLogService {
  private readonly List<IView> logMessages = [];

  public void Append(string message) {
    logMessages.Add(locale.CreateLog(message));
  }

  public IEnumerable<string> GetMessages() {
    return logMessages.SelectMany(view => view.ToWriter().Plain);
  }

  public void Clear() { logMessages.Clear(); }

  public void PrintLogs(CCSPlayerController? player) {
    if (player == null || !player.IsReal()) {
      locale.BeginJailbreakLogs.ToServerConsole();
      foreach (var log in logMessages) log.ToServerConsole();
      locale.EndJailbreakLogs.ToServerConsole();
      return;
    }


    locale.BeginJailbreakLogs.ToConsole(player);
    foreach (var log in logMessages) log.ToConsole(player);
    locale.EndJailbreakLogs.ToConsole(player);
  }

  public void Append(params FormatObject[] objects) {
    logMessages.Add(locale.CreateLog(objects));
  }

  public FormatObject Player(CCSPlayerController playerController) {
    return new TreeFormatObject {
      playerController,
      $"[{playerController.UserId}]",
      richPlayerTag.Rich(playerController)
    };
  }

  [GameEventHandler]
  public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info) {
    locale.BeginJailbreakLogs.ToServerConsole().ToAllConsole();

    //  By default, print all logs to player consoles at the end of the round.
    foreach (var log in logMessages) log.ToServerConsole().ToAllConsole();

    locale.EndJailbreakLogs.ToServerConsole().ToAllConsole();
    return HookResult.Continue;
  }

  [GameEventHandler]
  public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info) {
    Clear();
    return HookResult.Continue;
  }
}