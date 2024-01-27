using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.Warden;

public interface IWardenService
{

	CCSPlayerController? Warden { get; }

	/// <summary>
	/// Whether or not a warden is currently assigned
	/// </summary>
	bool HasWarden { get; }

	bool IsWarden(CCSPlayerController? player)
	{
		if (player == null || !player.IsValid)
			return false;
		return HasWarden && Warden != null  && Warden.Slot == player.Slot;
	}

	bool TrySetWarden(CCSPlayerController warden);

	bool TryRemoveWarden();

}
