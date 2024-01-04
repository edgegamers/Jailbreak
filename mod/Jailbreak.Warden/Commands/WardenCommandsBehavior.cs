using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;

using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Warden;

namespace Jailbreak.Warden.Commands;

public class WardenCommandsBehavior : IPluginBehavior
{
	private readonly IWardenSelectionService _queue;
	private readonly IWardenService _warden;

	public WardenCommandsBehavior(IWardenSelectionService queue, IWardenService warden)
	{
		_queue = queue;
		_warden = warden;
	}

	public void Dispose()
	{
	}

	public HookResult HandleWarden(CCSPlayerController sender)
	{
		var isCt = sender.GetTeam() == CsTeam.CounterTerrorist;

		//	Is a CT and queue is open
		if (isCt && _queue.Active)
		{
			if (_queue.InQueue(sender))
				if (_queue.TryEnter(sender))
					sender.PrintToChat("[Warden] You've joined the queue!");

			if (!_queue.InQueue(sender))
				if (_queue.TryExit(sender))
					sender.PrintToChat("[Warden] You've left the queue!");

			return HookResult.Handled;
		}

		//	Is a CT and there is no warden
		if (isCt && !_warden.HasWarden)
			_warden.TrySetWarden(sender);

		//	Respond to all other requests
		if (_warden.HasWarden)
			sender.PrintToChat($"[Warden] The current warden is {_warden.Warden.PlayerName}");
		else
			sender.PrintToChat("[Warden] There is currently no warden!");

		return HookResult.Handled;
	}

	public HookResult HandlePass(CCSPlayerController sender)
	{
		var isCt = sender.GetTeam() == CsTeam.CounterTerrorist;
		var isWarden = _warden.HasWarden && _warden.Warden?.Slot == sender.Slot;

		if (isWarden)
		{
			//	Handle warden pass
			Server.PrintToChatAll("[Warden] The warden has passed!");
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
}
