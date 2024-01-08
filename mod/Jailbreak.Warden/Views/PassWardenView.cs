using CounterStrikeSharp.API.Core;

using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;

namespace Jailbreak.Warden.Views;

public class PassWardenView : IView
{
	private CCSPlayerController _newWarden;

	public PassWardenView(CCSPlayerController newWarden)
	{
		_newWarden = newWarden;
	}

	public void Render(FormatWriter writer)
	{
		writer.Line(WardenNotifications.PREFIX, _newWarden, "has resigned from being warden!");
	}
}
