using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.Logs;

public class LogMessages : ILogMessages, ILanguage<Formatting.Languages.English>
{
    public IView BEGIN_JAILBREAK_LOGS => new SimpleView
    {
        "********************************", SimpleView.NEWLINE,
        "***** BEGIN JAILBREAK LOGS *****", SimpleView.NEWLINE,
        "********************************"
    };

    public IView END_JAILBREAK_LOGS => new SimpleView
    {
        "********************************", SimpleView.NEWLINE,
        "****** END JAILBREAK LOGS ******", SimpleView.NEWLINE,
        "********************************"
    };
}