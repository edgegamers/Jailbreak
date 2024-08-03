using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.Logs;

public class LogLocale : ILogLocale, ILanguage<Formatting.Languages.English> {
  public IView BeginJailbreakLogs
    => new SimpleView {
      "********************************",
      SimpleView.NEWLINE,
      "***** BEGIN JAILBREAK LOGS *****",
      SimpleView.NEWLINE,
      "********************************"
    };

  public IView EndJailbreakLogs
    => new SimpleView {
      "********************************",
      SimpleView.NEWLINE,
      "****** END JAILBREAK LOGS ******",
      SimpleView.NEWLINE,
      "********************************"
    };
}