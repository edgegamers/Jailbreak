using CounterStrikeSharp.API.Core;

using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;

namespace Jailbreak.Warden.Views;

public class CurrentWardenView : IView
{
	private CCSPlayerController? _warden;

	public CurrentWardenView(CCSPlayerController? warden)
	{
		_warden = warden;
	}

	public void Render(FormatWriter writer)
	{
		if (_warden is not null)
			writer.Line(WardenNotifications.PREFIX, "The current warden is", _warden);
		else
			writer.Line(WardenNotifications.PREFIX, "There is currently no warden!");
	}
}
