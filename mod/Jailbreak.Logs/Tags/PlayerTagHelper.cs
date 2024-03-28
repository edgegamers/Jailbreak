using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Objects;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Logs;
using Jailbreak.Public.Mod.Rebel;
using Jailbreak.Public.Mod.Warden;

using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Logs.Tags;

public class PlayerTagHelper : IRichPlayerTag, IPlayerTag
{
	private Lazy<IWardenService> _wardenService;
	private Lazy<IRebelService> _rebelService;
	private Lazy<ISpecialTreatmentService> _specialTreatmentService;

	public PlayerTagHelper(IServiceProvider provider)
	{
		//  Lazy-load dependencies to avoid loops, since we are a lower-level class.
		_wardenService = new ( () => provider.GetRequiredService<IWardenService>() );
		_rebelService =  new ( () => provider.GetRequiredService<IRebelService>() );
		_specialTreatmentService = new ( () => provider.GetRequiredService<ISpecialTreatmentService>() );
	}

	public FormatObject Rich(CCSPlayerController player)
	{
		if (_wardenService.Value.IsWarden(player))
			return new StringFormatObject("(WARDEN)", ChatColors.DarkBlue);
		if (player.GetTeam() == CsTeam.CounterTerrorist)
			return new StringFormatObject("(CT)", ChatColors.BlueGrey);
		if (_rebelService.Value.IsRebel(player))
			return new StringFormatObject("(REBEL)", ChatColors.DarkRed);
		if (_specialTreatmentService.Value.IsSpecialTreatment(player))
			return new StringFormatObject("(ST)", ChatColors.Green);

		return new StringFormatObject("(T)", ChatColors.Yellow);
	}

	public string Plain(CCSPlayerController playerController)
	{
		return Rich(playerController).ToPlain();
	}
}
