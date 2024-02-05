using CounterStrikeSharp.API.Modules.Utils;

using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Objects;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.Teams;

public class RatioNotifications : IRatioNotifications, ILanguage<Formatting.Languages.English>
{
	public static FormatObject PREFIX = new HiddenFormatObject($" {ChatColors.LightRed}[{ChatColors.Red}JB{ChatColors.LightRed}]")
	{
		//	Hide in panorama and center text
		Plain = false,
		Panorama = false,
		Chat = true
	};

	public IView NOT_ENOUGH_GUARDS =>
		new SimpleView { PREFIX, "There's not enough guards in the queue!" };

	public IView PLEASE_JOIN_GUARD_QUEUE =>
		new SimpleView { PREFIX, "Type !guard to become a guard!" };

	public IView JOINED_GUARD_QUEUE =>
		new SimpleView { PREFIX, "You've joined the guard queue!" };

	public IView ALREADY_A_GUARD =>
		new SimpleView { PREFIX, "You're already a guard!" };

	public IView YOU_WERE_AUTOBALANCED_PRISONER =>
		new SimpleView
		{
			{ PREFIX, "You were autobalanced to the prisoner team!" },
			{ PREFIX, "Please use !guard to join the guard team." }
		};

	public IView ATTEMPT_TO_JOIN_FROM_TEAM_MENU =>
		new SimpleView { PREFIX, "You were swapped back to the prisoner team!" };

	public IView LEFT_GUARD =>
		new SimpleView
		{
			{ PREFIX, "You are no longer a guard." },
			{ PREFIX, "Please use !guard if you want to re-join the guard team." }
		};

	public IView YOU_WERE_AUTOBALANCED_GUARD =>
		new SimpleView { PREFIX, "You are now a guard!" };
}
