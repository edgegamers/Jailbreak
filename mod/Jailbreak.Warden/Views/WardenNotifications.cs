using CounterStrikeSharp.API.Modules.Utils;

using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Objects;

namespace Jailbreak.Warden.Views;

public static class WardenNotifications
{
	public static FormatObject PREFIX = new HiddenFormatObject( $" {ChatColors.Lime}[{ChatColors.Green}WARDEN{ChatColors.Lime}]" )
	{
		//	Hide in panorama and center text
		Plain = false,
		Panorama = false,
		Chat = true,
	};

	public static IView PICKING_SHORTLY => new SimpleView(writer =>
		writer
			.Line(PREFIX, "Picking a warden shortly")
			.Line(PREFIX, "To enter the warden queue, type !warden in chat."));

	public static IView NO_WARDENS => new SimpleView(writer =>
		writer
			.Line(PREFIX, "No wardens in queue! The next player to run !warden will become a warden."));

	public static IView WARDEN_LEFT => new SimpleView(writer =>
		writer.Line(PREFIX, "The warden has left the game!"));

	public static IView WARDEN_DIED => new SimpleView(writer =>
		writer.Line(PREFIX, "The warden has died!"));

	public static IView BECOME_NEXT_WARDEN => new SimpleView(writer =>
		writer.Line(PREFIX, "Type !warden to become the next warden"));

	public static IView JOIN_RAFFLE => new SimpleView(writer =>
		writer.Line(PREFIX, "You've joined the warden raffle!"));

	public static IView LEAVE_RAFFLE => new SimpleView(writer =>
		writer.Line(PREFIX, "You've left the warden raffle!"));

}
