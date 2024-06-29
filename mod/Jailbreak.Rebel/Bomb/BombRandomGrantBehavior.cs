using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;

using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Generic;
using Jailbreak.Public.Mod.Rebel;

using Serilog;

namespace Jailbreak.Rebel.Bomb;

public class BombRandomGrantBehavior : IPluginBehavior
{
	private readonly ICoroutines _coroutines;
	private readonly IBombNotifications _notifications;

	private readonly IBombService _bomb;

	public BombRandomGrantBehavior(ICoroutines coroutines, IBombNotifications notifications, IBombService bomb)
	{
		_coroutines = coroutines;
		_notifications = notifications;
		_bomb = bomb;
	}

	[GameEventHandler(HookMode.Post)]
	public HookResult OnRoundStart(EventRoundStart ev, GameEventInfo info)
	{
		_coroutines.Round(Grant, 3.0f);

		return HookResult.Continue;
	}

	private void Grant()
	{
		var players = Utilities.GetPlayers()
			.Where(player => player.GetTeam() == CsTeam.Terrorist)
			.Where(player => player.PawnIsAlive)
			.ToList();

		//	ugh
		if (players.Count == 0)
			return;

		var chosenIdx = Random.Shared.Next(players.Count);
		var chosen = players[chosenIdx];

		if (_bomb.TryGrant(chosen))
		{
			_notifications.BOMB_RECEIVED.ToPlayerChat(chosen)
				.ToPlayerCenter(chosen);

			_notifications.BOMB_USAGE.ToPlayerChat(chosen);
		}
		else
		{
			Log.Warning("Could not grant a player The Bomb! Why???");
		}
	}
}
