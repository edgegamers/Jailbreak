using System;
using System.Linq;
using System.Runtime.CompilerServices;

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;

using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Generic;
using Jailbreak.Public.Mod.Warden;

using Serilog;

using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.Warden.Selection;

/// <summary>
/// Behavior responsible for choosing the next warden
/// </summary>
public class WardenSelectionBehavior : IPluginBehavior, IWardenSelectionService
{
	/// <summary>
	/// A state dict that handles the player's current queue
	/// enrollment and favor. Uses the Round reset mode.
	/// </summary>
	private IPlayerState<QueueState> _queue;

	private IPlayerState<QueueFavorState> _favor;

	/// <summary>
	/// Whether or not to use the queue.
	/// When true, the queue should be skipped and turn to first-come-first-serve.
	/// </summary>
	private bool _queueInactive;

	private IWardenService _warden;

	private IWardenNotifications _notifications;

	public WardenSelectionBehavior(IPlayerStateFactory factory, IWardenService warden, IWardenNotifications notifications)
	{
		_warden = warden;
		_notifications = notifications;
		_queue = factory.Round<QueueState>();
		_favor = factory.Global<QueueFavorState>();

		_queueInactive = true;
	}

	public void Start(BasePlugin parent)
	{

	}

	public void Dispose()
	{

	}

	[GameEventHandler]
	public HookResult OnRoundStart(EventRoundStart ev, GameEventInfo info)
	{
		//	Enable the warden queue
		_queueInactive = false;

		_notifications.PICKING_SHORTLY.ToAllChat();

		//	Start a timer to pick the warden in 7 seconds
		ScheduleChooseWarden(7.0f);

		return HookResult.Continue;
	}

	[MethodImpl(MethodImplOptions.NoOptimization)]
	public void ScheduleChooseWarden(float time = 5.0f)
	{
		var timer = new Timer(time, OnChooseWarden, TimerFlags.STOP_ON_MAPCHANGE);
	}

	/// <summary>
	/// Timer callback that states it's time to choose the warden.
	/// </summary>
	protected void OnChooseWarden()
	{
		var eligible = Utilities.GetPlayers()
			.Where(player => player.PawnIsAlive)
			.Where(player => player.GetTeam() == CsTeam.CounterTerrorist)
			.Where(player => _queue.Get(player).InQueue)
			.ToList();

		Log.Information("[WardenSelectionBehavior] Picking warden from {@Eligible}", eligible);

		if (eligible.Count == 0)
		{
			_notifications.NO_WARDENS.ToAllChat();
			_queueInactive = true;

			return;
		}

		var favors = eligible
			.ToDictionary(player => player, player => _favor.Get(player));
		int tickets = favors.Sum(favor => favor.Value.GetTickets());
		int chosen = Random.Shared.Next(tickets);

		Server.PrintToConsole($"[Warden Raffle] Picking {chosen} out of {tickets}");

		int pointer = 0;
		foreach (var (player, favor) in favors)
		{
			int thisTickets = favor.GetTickets();
			Server.PrintToConsole($"[Warden Raffle] {pointer} -> {pointer + thisTickets}: #{player.Slot} {player.PlayerName}");

			//	If winning ticket belongs to this player, assign them as warden.
			if (pointer <= chosen && chosen < (pointer + thisTickets))
			{
				_warden.TrySetWarden(player);
				favor.RoundsWithoutWarden = 0;
			}
			else
			{
				favor.RoundsWithoutWarden++;
			}

			pointer += thisTickets;
		}

		//	Disable the warden raffle for future wardens
		//	(eg in the event of warden death)
		_queueInactive = true;
	}

	private bool CanEnterQueue(CCSPlayerController player)
	{
		if (player.GetTeam() != CsTeam.CounterTerrorist)
			return false;

		if (!player.PawnIsAlive)
			return false;

		//	Cannot enter queue if queue is not running
		if (_queueInactive)
			return false;

		//	Edge case: Is there already a warden?
		if (_warden.HasWarden)
			return false;

		return true;
	}

	public bool TryEnter(CCSPlayerController player)
	{
		if (!CanEnterQueue(player))
			return false;

		_queue.Get(player).InQueue = true;
		return true;
	}

	public bool TryExit(CCSPlayerController player)
	{
		if (!CanEnterQueue(player))
			return false;

		_queue.Get(player).InQueue = false;
		return true;
	}

	public bool InQueue(CCSPlayerController player)
		=>  _queue.Get(player).InQueue;

	public bool Active => !_queueInactive;


}
