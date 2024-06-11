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

public class BombRandomGrantBehavior : IPluginBehavior, IBombResultHook
{
	private readonly ICoroutines _coroutines;
	private readonly IJihadC4Notifications _notifications;

	private readonly IBombService _bomb;

	/*private Dictionary<string, int> _damageCounts = new();
	private Dictionary<string, int> _flunkCounts = new();*/

	public BombRandomGrantBehavior(ICoroutines coroutines, IJihadC4Notifications notifications, IBombService bomb)
	{
		_coroutines = coroutines;
		_notifications = notifications;
		_bomb = bomb;
	}

	/*[GameEventHandler]
	public HookResult OnGameStart(EventGameStart ev, GameEventInfo info)
	{
		_damageCounts.Clear();
		_flunkCounts.Clear();

		return HookResult.Continue;
	}*/

	[GameEventHandler(HookMode.Post)]
	public HookResult OnRoundStart(EventRoundStart ev, GameEventInfo info)
	{
		_coroutines.Round(Grant, 3.0f);

		return HookResult.Continue;
	}

	/*private float CalculatePlayerScore(string steamId)
	{
		float score = 1f;

		if (_flunkCounts.ContainsKey(steamId))
			score /= (_flunkCounts[steamId] * _flunkCounts[steamId]);

		if (_damageCounts.ContainsKey(steamId))
			score += (_damageCounts[steamId] / 1000);

		return score;
	}*/

	private void Grant()
	{
		var players = Utilities.GetPlayers()
			.Where(player => player.GetTeam() == CsTeam.Terrorist)
			.Where(player => player.PawnIsAlive)
			.ToList();

		var chosenIdx = Random.Shared.Next(players.Count);
		var chosen = players[chosenIdx];

		if (_bomb.TryGrant(chosen))
		{
			_notifications.JIHAD_C4_RECEIVED.ToPlayerChat(chosen)
				.ToPlayerCenter(chosen);

			_notifications.JIHAD_C4_USAGE.ToPlayerChat(chosen);
		}
		else
		{
			Log.Warning("Could not grant player C4! Why???");
		}
	}

	public void OnDetonation(BombResult result)
	{
		/*
		if (result.Damage > 30 && result.Kills == 0)
		{
			//	Flunk! Punish the player
			if (result.Bomber.IsReal())
				_notifications.BOMB_FLUNK.ToPlayerChat(result.Bomber);

			if (!_flunkCounts.ContainsKey(result.SteamId))
				_flunkCounts.Add(result.SteamId, 0);

			_flunkCounts[result.SteamId]++;
			return;
		}

		if (!_damageCounts.ContainsKey(result.SteamId))
			_damageCounts.Add(result.SteamId, 0);

		//	Double credit for actual kills
		_damageCounts[result.SteamId] += result.Damage + (100 * result.Kills);
		*/
	}
}
