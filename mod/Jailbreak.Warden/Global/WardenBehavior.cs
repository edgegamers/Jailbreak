using System.Reflection;

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;

using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Warden;

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

		Server.PrintToChatAll($"[Warden] {_warden.PlayerName.Sanitize()} is now the warden!");
		ServerExtensions.PrintToCenterAll($"{_warden.PlayerName.Sanitize()} is now the warden!");
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
			Server.PrintToChatAll("[Warden] The current warden has died!");
			Server.PrintToChatAll("[Warden] Type !warden to become the next warden");
			ServerExtensions.PrintToCenterAll("The warden has died!");
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

			//	Warden died!
			Server.PrintToChatAll("[Warden] The current warden has left the game!");
			Server.PrintToChatAll("[Warden] Type !warden to become the next warden");
			ServerExtensions.PrintToCenterAll("The warden has left!");
		}

		return HookResult.Continue;
	}
}
