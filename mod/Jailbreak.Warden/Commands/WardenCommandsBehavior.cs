using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
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

	public HookResult HandleWarden(CCSPlayerController sender)
	{
		var isCt = sender.GetTeam() == CsTeam.CounterTerrorist;

		//	Is a CT and queue is open
		if (isCt && _queue.Active)
		{
			if (_queue.InQueue(sender))
				if (_queue.TryEnter(sender))
					_notifications.JOIN_RAFFLE.ToPlayerChat(sender);

			if (!_queue.InQueue(sender))
				if (_queue.TryExit(sender))
					_notifications.LEAVE_RAFFLE.ToPlayerChat(sender);

			return HookResult.Handled;
		}

		//	Is a CT and there is no warden
		if (isCt && !_warden.HasWarden)
			_warden.TrySetWarden(sender);

		_notifications.CURRENT_WARDEN(_warden.Warden).ToPlayerChat(sender);

		return HookResult.Handled;
	}

	public HookResult HandlePass(CCSPlayerController sender)
	{
		var isCt = sender.GetTeam() == CsTeam.CounterTerrorist;
		var isWarden = _warden.HasWarden && _warden.Warden?.Slot == sender.Slot;

		if (isWarden)
		{
			//	Handle warden pass
			_notifications.PASS_WARDEN(sender)
				.ToAllChat()
				.ToAllCenter();

			_notifications.BECOME_NEXT_WARDEN.ToAllChat();

			if (!_warden.TryRemoveWarden())
				Server.PrintToChatAll("[BUG] Couldn't remove warden :^(");

			return HookResult.Handled;
		}

		return HookResult.Continue;
	}

	[GameEventHandler(HookMode.Pre)]
	public HookResult OnPlayerChat(EventPlayerChat chat, GameEventInfo info)
	{
		var sender = Utilities.GetPlayerFromUserid(chat.Userid);
		var command = chat.Text.ToLowerInvariant();

		if (!chat.Text.StartsWith("!"))
			return HookResult.Continue;

		//	Player is not sending a warden command
		if (command == "!w" || command == "!warden")
			return HandleWarden(sender);

		if (command == "!uw" || command == "!unwarden" || command == "!pass")
			return HandlePass(sender);

		return HookResult.Continue;
	}

	public void Dispose()
	{
	}
}
