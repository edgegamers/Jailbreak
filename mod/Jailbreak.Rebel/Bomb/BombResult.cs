using CounterStrikeSharp.API.Core;

namespace Jailbreak.Rebel.Bomb;

public class BombResult
{
	public CCSPlayerController? Bomber { get; set; }
	public string SteamId { get; set; }

	public int Damage { get; set; }
	public int Kills { get; set; }
}
