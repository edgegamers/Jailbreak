using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Logs;
using Jailbreak.Public.Mod.Warden;
using Microsoft.Extensions.Logging;

namespace Jailbreak.Warden.Global;

public class WardenBehavior : IPluginBehavior, IWardenService
{
	private ILogger<WardenBehavior> _logger;
	private IRichLogService logs;

	private IWardenNotifications _notifications;

	private bool _hasWarden;
	private CCSPlayerController? _warden;

	public WardenBehavior(ILogger<WardenBehavior> logger, IWardenNotifications notifications, IRichLogService logs)
	{
		_logger = logger;
		_notifications = notifications;
		logs = logs;
	}

	/// <summary>
	/// Get the current warden, if there is one.
	/// </summary>
	public CCSPlayerController? Warden => _warden;

	/// <summary>
	/// Whether or not a warden is currently assigned
	/// </summary>
	public bool HasWarden => _hasWarden;

	public bool TrySetWarden(CCSPlayerController controller)
	{
		if (_hasWarden)
			return false;

		//	Verify player is a CT
		if (controller.GetTeam() != CsTeam.CounterTerrorist)
			return false;
		if (!controller.PawnIsAlive)
			return false;

		_hasWarden = true;
		_warden = controller;

		if (_warden.Pawn.Value != null)
		{
			_warden.Pawn.Value.RenderMode = RenderMode_t.kRenderTransColor;
		    _warden.Pawn.Value.Render = Color.Blue;
			Utilities.SetStateChanged(_warden.Pawn.Value, "CBaseModelEntity", "m_clrRender");
		}

		_notifications.NEW_WARDEN(_warden)
			.ToAllChat()
			.ToAllCenter();

		logs.Append( logs.Player(_warden), "is now the warden.");
		return true;
	}

	public bool TryRemoveWarden()
	{
		if (!_hasWarden)
			return false;

		_hasWarden = false;

		if (_warden != null && _warden.Pawn.Value != null)
		{
			_warden.Pawn.Value.RenderMode = RenderMode_t.kRenderTransColor;
			_warden.Pawn.Value.Render = Color.FromArgb(254, 255, 255, 255);
			Utilities.SetStateChanged(_warden.Pawn.Value, "CBaseModelEntity", "m_clrRender");
			logs.Append( logs.Player(_warden), "is no longer the warden.");
		}

		_warden = null;

		return true;
	}

	[GameEventHandler]
	public HookResult OnDeath(EventPlayerDeath ev, GameEventInfo info)
	{
		if (!_hasWarden)
			return HookResult.Continue;

		if (ev.Userid.UserId == _warden.UserId)
		{
			if (!this.TryRemoveWarden())
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
		this.TryRemoveWarden();

		return HookResult.Continue;
	}

	[GameEventHandler]
	public HookResult OnPlayerDisconnect(EventPlayerDisconnect ev, GameEventInfo info)
	{
		if (!_hasWarden)
			return HookResult.Continue;

		if (ev.Userid.UserId == _warden.UserId)
		{
			if (!this.TryRemoveWarden())
				_logger.LogWarning("[Warden] BUG: Problem removing current warden :^(");


			_notifications.WARDEN_LEFT
				.ToAllChat()
				.ToAllCenter();

			_notifications.BECOME_NEXT_WARDEN.ToAllChat();
		}

		return HookResult.Continue;
	}
}
