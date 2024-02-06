using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;

using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Objects;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Extensions;

namespace Jailbreak.English.Logs;

public class LogMessages : ILogMessages, ILanguage<Formatting.Languages.English>
{



	public IView BEGIN_JAILBREAK_LOGS => new SimpleView()
	{
		{ "********************************" },
		{ "***** BEGIN JAILBREAK LOGS *****" },
		{ "********************************" }
	};

	public IView END_JAILBREAK_LOGS => new SimpleView()
	{
		{ "********************************" },
		{ "****** END JAILBREAK LOGS ******" },
		{ "********************************" }
	};



}
