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

public class LogsManager(ILogMessages messages, IRichPlayerTag richPlayerTag)
  : IPluginBehavior, IRichLogService {
  private readonly List<IView> logMessages = [];

  public void Append(string message) {
    logMessages.Add(messages.CreateLog(message));
  }

  public IEnumerable<string> GetMessages() {
    return logMessages.SelectMany(view => view.ToWriter().Plain);
  }

  public void Clear() { logMessages.Clear(); }

  public void PrintLogs(CCSPlayerController? player) {
    if (player == null || !player.IsReal()) {
      messages.BeginJailbreakLogs.ToServerConsole();
      foreach (var log in logMessages) log.ToServerConsole();
      messages.EndJailbreakLogs.ToServerConsole();
      return;
    }


    messages.BeginJailbreakLogs.ToPlayerConsole(player);
    foreach (var log in logMessages) log.ToPlayerConsole(player);
    messages.EndJailbreakLogs.ToPlayerConsole(player);
  }

  public void Append(params FormatObject[] objects) {
    logMessages.Add(messages.CreateLog(objects));
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
    messages.BeginJailbreakLogs.ToServerConsole().ToAllConsole();

    //  By default, print all logs to player consoles at the end of the round.
    foreach (var log in logMessages) log.ToServerConsole().ToAllConsole();

    messages.EndJailbreakLogs.ToServerConsole().ToAllConsole();
    return HookResult.Continue;
  }

  [GameEventHandler]
  public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info) {
    Clear();
    return HookResult.Continue;
  }
}