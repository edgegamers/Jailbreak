using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Warden;

namespace Jailbreak.Warden.Commands;

public class WardenCommandsBehavior : IPluginBehavior
{
	private IWardenSelectionService _queue;
	private IWardenService _warden;
	private IWardenNotifications _notifications;

	public WardenCommandsBehavior(IWardenSelectionService queue, IWardenService warden, IWardenNotifications notifications)
	{
		_queue = queue;
		_warden = warden;
		_notifications = notifications;
	}


	[ConsoleCommand("css_pass", "Pass warden onto another player")]
	[ConsoleCommand("css_uw", "Pass warden onto another player")]
	[CommandHelper(0, "", CommandUsage.CLIENT_ONLY)]
	public void Command_Pass(CCSPlayerController? player, CommandInfo command)
	{
		if (player == null)
			return;

		var isCt = player.GetTeam() == CsTeam.CounterTerrorist;

		if (_warden.IsWarden(player))
		{
			//	Handle warden pass
			_notifications.PASS_WARDEN(player)
				.ToAllChat()
				.ToAllCenter();

			_notifications.BECOME_NEXT_WARDEN.ToAllChat();

			if (!_warden.TryRemoveWarden())
				Server.PrintToChatAll("[BUG] Couldn't remove warden :^(");
		}
	}

	[ConsoleCommand("css_warden", "Become a warden, Join the warden queue, or see information about the current warden.")]
	[ConsoleCommand("css_w", "Become a warden, Join the warden queue, or see information about the current warden.")]
	[CommandHelper(0, "", CommandUsage.CLIENT_ONLY)]
	public void Command_Warden(CCSPlayerController? player, CommandInfo command)
	{
		if (player == null)
			return;

		var isCt = player.GetTeam() == CsTeam.CounterTerrorist;

		//	Is a CT and queue is open
		if (isCt && _queue.Active)
		{
			if (!_queue.InQueue(player)) {
				if (_queue.TryEnter(player))
					_notifications.JOIN_RAFFLE.ToPlayerChat(player);
				return;
			}

			if (_queue.InQueue(player))
				if (_queue.TryExit(player))
					_notifications.LEAVE_RAFFLE.ToPlayerChat(player);

			return;
		}

		//	Is a CT and there is no warden
		if (isCt && !_warden.HasWarden)
			_warden.TrySetWarden(player);

		_notifications.CURRENT_WARDEN(_warden.Warden).ToPlayerChat(player);
	}

	public void Dispose()
	{
	}
}
