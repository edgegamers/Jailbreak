using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Objects;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.Warden;

public class SpecialTreatmentNotifications : ISpecialTreatmentNotifications, ILanguage<Formatting.Languages.English>
{
	public static FormatObject PREFIX = new HiddenFormatObject( $" {ChatColors.Lime}[{ChatColors.Green}ST{ChatColors.Lime}]" )
	{
		//	Hide in panorama and center text
		Plain = false,
		Panorama = false,
		Chat = true,
	};

	public IView GRANTED =>
		new SimpleView { PREFIX, "You now have special treatment!" };

	public IView REVOKED =>
		new SimpleView { PREFIX, "Your special treatment was removed" };

	public IView GRANTED_TO(CCSPlayerController player)
	{
		return new SimpleView { PREFIX, player, "now has Special Treatment!" };
	}

	public IView REVOKED_FROM(CCSPlayerController player)
	{
		return new SimpleView { PREFIX, player, "no longer has Special Treatment." };
	}

}
