using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Objects;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Logs;
using Jailbreak.Public.Mod.Rebel;
using Jailbreak.Public.Mod.Warden;

using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Logs.Tags;

public class PlayerTagHelper : IRichPlayerTag, IPlayerTag
{
	private IWardenService _wardenService;
	private IRebelService _rebelService;

	public PlayerTagHelper(IServiceProvider provider)
	{
		//  Lazy-load dependencies to avoid loops, since we are a lower-level class.
		_wardenService = provider.GetRequiredService<IWardenService>();
		_rebelService = provider.GetRequiredService<IRebelService>();
	}

	public FormatObject Rich(CCSPlayerController player)
	{
		if (_wardenService.IsWarden(player))
			return new StringFormatObject("(WARDEN)", ChatColors.DarkBlue);
		if (player.GetTeam() == CsTeam.CounterTerrorist)
			return new StringFormatObject("(CT)", ChatColors.BlueGrey);
		if (_rebelService.IsRebel(player))
			return new StringFormatObject("(REBEL)", ChatColors.Darkred);

		return new StringFormatObject("(T)", ChatColors.Yellow);
	}

	public string Plain(CCSPlayerController playerController)
	{
		return $"{playerController.PlayerName} [#{playerController.UserId}] {Rich(playerController).ToPlain()}";
	}
}
