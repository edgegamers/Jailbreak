using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;

using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Warden;

using Microsoft.Extensions.Logging;

namespace Jailbreak.Warden.Global;

public class WardenBehavior : IPluginBehavior, IWardenService
{
	private readonly ILogger<WardenBehavior> _logger;

	public WardenBehavior(ILogger<WardenBehavior> logger)
	{
		_logger = logger;
	}

	/// <summary>
	///     Get the current warden, if there is one.
	/// </summary>
	public CCSPlayerController? Warden { get; private set; }

	/// <summary>
	///     Whether or not a warden is currently assigned
	/// </summary>
	public bool HasWarden { get; private set; }

	public bool TrySetWarden(CCSPlayerController controller)
	{
		if (HasWarden)
			return false;

		//	Verify player is a CT
		if (controller.GetTeam() != CsTeam.CounterTerrorist)
			return false;

		HasWarden = true;
		Warden = controller;

		Server.PrintToChatAll($"[Warden] {Warden.PlayerName.Sanitize()} is now the warden!");
		ServerExtensions.PrintToCenterAll($"{Warden.PlayerName.Sanitize()} is now the warden!");
		Warden.ClanName = "[WARDEN]";

		return true;
	}

	public bool TryRemoveWarden()
	{
		if (!HasWarden)
			return false;

		if (Warden != null)
			Warden.ClanName = "";

		HasWarden = false;
		Warden = null;

		return true;
	}

	[GameEventHandler]
	public HookResult OnDeath(EventPlayerDeath ev, GameEventInfo info)
	{
		if (!HasWarden)
			return HookResult.Continue;

		if (ev.Userid.UserId == Warden.UserId)
		{
			if (!TryRemoveWarden())
				_logger.LogWarning("[Warden] BUG: Problem removing current warden :^(");

			//	Warden died!
			Server.PrintToChatAll("[Warden] The current warden has died!");
			Server.PrintToChatAll("[Warden] Type !warden to become the next warden");
			ServerExtensions.PrintToCenterAll("The warden has died!");
		}

		return HookResult.Continue;
	}

	[GameEventHandler]
	public HookResult OnRoundEnd(EventRoundEnd ev, GameEventInfo info)
	{
		TryRemoveWarden();

		return HookResult.Continue;
	}

	[GameEventHandler]
	public HookResult OnPlayerDisconnect(EventPlayerDisconnect ev, GameEventInfo info)
	{
		if (!HasWarden)
			return HookResult.Continue;

		if (ev.Userid.UserId == Warden.UserId)
		{
			if (!TryRemoveWarden())
				_logger.LogWarning("[Warden] BUG: Problem removing current warden :^(");

			//	Warden died!
			Server.PrintToChatAll("[Warden] The current warden has left the game!");
			Server.PrintToChatAll("[Warden] Type !warden to become the next warden");
			ServerExtensions.PrintToCenterAll("The warden has left!");
		}

		return HookResult.Continue;
	}
}
