using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;

using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Warden;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;

using Microsoft.Extensions.Logging;

namespace Jailbreak.Warden.Global;

public class WardenBehavior : IPluginBehavior, IWardenService
{
	private readonly ILogger<WardenBehavior> _logger;

	public WardenBehavior(ILogger<WardenBehavior> logger)
	{
		_logger = logger;
	}


	private IWardenNotifications _notifications;

	private bool _hasWarden;
	private CCSPlayerController? _warden;

	public WardenBehavior(IWardenNotifications notifications)
	{
		_notifications = notifications;
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

		_notifications.NEW_WARDEN(_warden)
			.ToAllChat()
			.ToAllCenter();

		_warden.ClanName = "[WARDEN]";

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
			_notifications.WARDEN_DIED
				.ToAllChat()
				.ToAllCenter();

			_notifications.BECOME_NEXT_WARDEN.ToAllChat();
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


			_notifications.WARDEN_LEFT
				.ToAllChat()
				.ToAllCenter();

			_notifications.BECOME_NEXT_WARDEN.ToAllChat();
		}

		return HookResult.Continue;
	}
}
