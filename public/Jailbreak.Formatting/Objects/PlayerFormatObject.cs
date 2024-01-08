using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

using Jailbreak.Public.Extensions;

namespace Jailbreak.Formatting.Formatting;

public class PlayerFormatObject : FormatObject
{
	private CCSPlayerController _player;

	public PlayerFormatObject(CCSPlayerController player)
	{
		_player = player;
	}

	public override string ToChat()
		=> $"{ChatColors.Yellow}{_player.PlayerName}";

	public override string ToPanorama()
		=> _player.PlayerName.Sanitize();

	public override string ToPlain()
		=> _player.PlayerName;
}
