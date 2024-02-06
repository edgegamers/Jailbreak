using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;

namespace Jailbreak.Formatting.Extensions;

public static class ViewExtensions
{

	public static FormatWriter ToWriter(this IView view)
	{
		var writer = new FormatWriter();

		view.Render(writer);

		return writer;
	}

	public static IView ToServerConsole(this IView view)
	{
		var writer = view.ToWriter();

		foreach (string s in writer.Plain)
		{
			Server.PrintToConsole(s);
		}

		return view;
	}

	#region Individual

	public static IView ToPlayerConsole(this IView view, CCSPlayerController player)
	{
		var writer = view.ToWriter();

		foreach (string writerLine in writer.Plain)
			player.PrintToConsole(writerLine);

		return view;
	}

	public static IView ToPlayerChat(this IView view, CCSPlayerController player)
	{
		var writer = view.ToWriter();

		foreach (string writerLine in writer.Chat)
			player.PrintToChat(writerLine);

		return view;
	}

	public static IView ToPlayerCenter(this IView view, CCSPlayerController player)
	{
		var writer = view.ToWriter();
		var merged = string.Join('\n', writer.Plain);

		player.PrintToCenter(merged);

		return view;
	}

	public static IView ToPlayerCenterHtml(this IView view, CCSPlayerController player)
	{
		var writer = view.ToWriter();
		var merged = string.Join('\n', writer.Panorama);

		player.PrintToCenterHtml(merged);

		return view;
	}

	#endregion

	public static IView ToAllConsole(this IView view)
	{
		Utilities.GetPlayers().ForEach(player => view.ToPlayerConsole(player));

		return view;
	}

	public static IView ToAllChat(this IView view)
	{
		Utilities.GetPlayers().ForEach(player => view.ToPlayerChat(player));

		return view;
	}

	public static IView ToAllCenter(this IView view)
	{
		Utilities.GetPlayers().ForEach(player => view.ToPlayerCenter(player));

		return view;
	}
}
