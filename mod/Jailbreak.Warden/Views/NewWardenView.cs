using CounterStrikeSharp.API.Core;

using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;

namespace Jailbreak.Warden.Views;

public class NewWardenView : IView
{
	private CCSPlayerController _newWarden;

	public NewWardenView(CCSPlayerController newWarden)
	{
		_newWarden = newWarden;
	}

	public void Render(FormatWriter writer)
	{
		writer.Line(WardenNotifications.PREFIX, _newWarden, "is now the warden!");
	}
}
