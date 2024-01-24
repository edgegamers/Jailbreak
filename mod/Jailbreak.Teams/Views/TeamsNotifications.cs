using CounterStrikeSharp.API.Modules.Utils;

using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Objects;

namespace Jailbreak.Teams.Views;

public static class TeamsNotifications
{
	public static FormatObject PREFIX = new HiddenFormatObject( $" {ChatColors.LightRed}[{ChatColors.Red}JB{ChatColors.LightRed}]" )
	{
		//	Hide in panorama and center text
		Plain = false,
		Panorama = false,
		Chat = true,
	};

	public static IView NOT_ENOUGH_GUARDS = new SimpleView(writer =>
		writer
			.Line(PREFIX, "There's not enough guards in the queue!"));

	public static IView JOIN_GUARD_QUEUE = new SimpleView(writer =>
		writer
			.Line(PREFIX, "Type !guard to become a guard!"));

	public static IView YOU_WERE_AUTOBALANCED_PRISONER = new SimpleView(writer =>
		writer
			.Line(PREFIX, "You were autobalanced to the prisoner team!"));

	public static IView ATTEMPT_TO_JOIN_FROM_TEAM_MENU = new SimpleView(writer =>
		writer
			.Line(PREFIX, "You were swapped back to the prisoner team!")
			.Line(PREFIX, "Please use !guard to join the guard team."));

	public static IView LEFT_GUARD = new SimpleView(writer =>
		writer
			.Line(PREFIX, "You are no longer a guard.")
			.Line(PREFIX, "Please use !guard if you want to re-join the guard team."));

	public static IView YOU_WERE_AUTOBALANCED_GUARD = new SimpleView(writer =>
		writer
			.Line(PREFIX, "You are now a guard!"));
}
