using System.Reflection;

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;

using Jailbreak.Formatting.Formatting;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Warden;
using Jailbreak.Warden.Views;

namespace Jailbreak.Warden.Global;

public class WardenBehavior : IPluginBehavior, IWardenService
{

	public void Dispose()
	{
	}

	private bool _hasWarden;
	private CCSPlayerController? _warden;

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

		_hasWarden = true;
		_warden = controller;

		new NewWardenView(_warden)
			.ToAllChat()
			.ToAllCenter();

		_warden.ClanName = "[WARDEN]";

		return true;
	}

	public bool TryRemoveWarden()
	{
		if (!_hasWarden)
			return false;

		if (_warden != null)
			_warden.ClanName = "";

		_hasWarden = false;
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
				Server.PrintToConsole("[Warden] BUG: Problem removing current warden :^(");

			//	Warden died!
			WardenNotifications.WARDEN_DIED
				.ToAllChat()
				.ToAllCenter();

			WardenNotifications.BECOME_NEXT_WARDEN.ToAllChat();
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
				Server.PrintToConsole("[Warden] BUG: Problem removing current warden :^(");


			WardenNotifications.WARDEN_LEFT
				.ToAllChat()
				.ToAllCenter();

			WardenNotifications.BECOME_NEXT_WARDEN.ToAllChat();
		}

		return HookResult.Continue;
	}
}
