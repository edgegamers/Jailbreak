using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Generic;
using Jailbreak.Public.Mod.Teams;

using Serilog;

namespace Jailbreak.Teams.Queue;

public class QueueBehavior : IGuardQueue, IPluginBehavior
{
	private int _counter;
	private IPlayerState<QueueState> _state;

	public QueueBehavior(IPlayerStateFactory factory)
	{
		_counter = 0;
		_state = factory.Global<QueueState>();
	}

	public bool TryEnterQueue(CCSPlayerController player)
	{
		if (player.GetTeam() == CsTeam.CounterTerrorist)
			return false;

		var state = _state.Get(player);
		state.Position = ++_counter;
		state.InQueue = true;
		state.IsGuard = false;

		return true;
	}

	public bool TryExitQueue(CCSPlayerController player)
	{
		var state = _state.Get(player);
		state.InQueue = false;

		return true;
	}

	public bool TryPop(int count)
	{
		Server.PrintToChatAll($"[Jail] Autobalancing is adding {count} guards.");
		var queue = Queue.ToList();

		if (queue.Count <= count)
		{
			Server.PrintToChatAll("[Jail] Not enough guards are in the queue!");
			Server.PrintToChatAll("[Jail] Type !guard in chat to join the queue");
			ServerExtensions.PrintToCenterAll("Type !guard to become a guard!");
		}

		Log.Information($"[Queue] {count}/{queue.Count}");
		for (int i = 0; i < Math.Min(queue.Count, count); i++)
		{
			Log.Information($"[Queue] Popping { queue[i].PlayerName }");

			ForceGuard( queue[i] );
		}

		return true;
	}

	public bool TryPush(int count)
	{
		Server.PrintToChatAll($"[Jail] Autobalancing is removing {count} guards.");

		var players = Utilities.GetPlayers()
			.Where(player => player.GetTeam() == CsTeam.CounterTerrorist)
			.Shuffle(Random.Shared)
			.ToList();
		Log.Information($"[Queue] {count}/{players.Count}");

		for (int i = 0; i < Math.Min(count, players.Count); i++)
		{
			var toSwap = players[i];
			Log.Information($"[Queue] Pushing {toSwap.PlayerName}");
			var state = _state.Get(toSwap);

			state.IsGuard = false;
			toSwap.ChangeTeam(CsTeam.Terrorist);

			TryEnterQueue(toSwap);

			toSwap.PrintToCenter("You were autobalanced to the prisoner team!");
		}

		return true;
	}

	public void ForceGuard(CCSPlayerController player)
	{
		//	Set IsGuard so they won't be swapped back.
		_state.Get(player).IsGuard = true;

		player.PrintToCenter("You are now a guard!");
		player.ChangeTeam(CsTeam.CounterTerrorist);
	}

	[GameEventHandler]
	public HookResult OnPlayerTeam(EventPlayerTeam ev, GameEventInfo info)
	{
		var state = _state.Get(ev.Userid);
		var player = ev.Userid;

		if (ev.Team == (int)CsTeam.CounterTerrorist && !state.IsGuard)
		{            
			return HookResult.Handled;
		}

		if (player.GetTeam() == CsTeam.Terrorist && state.IsGuard)
		{
            HandleLeaveRequest(player);
		}

		return HookResult.Continue;
	}

    private void HandleQueueRequest(CCSPlayerController player) {
        if (TryEnterQueue(player))
            player.PrintToCenter("You were added to the CT queue!");
        else
            player.PrintToCenter("An error occured adding you to the queue.");
        
    }

    private void HandleLeaveRequest(CCSPlayerController player)
    {
        if (TryExitQueue(player)) 
            player.PrintToCenter("You were removed from the guard queue for switching to T.\nUse !guard to rejoin the queue!");
        else
            player.PrintToCenter("An error occured removing you from the queue.");
    }

    public int GetQueuePosition(CCSPlayerController player)
	{
		return Queue.ToList()
			.FindIndex(controller => controller.Slot == player.Slot);
	}

    [ConsoleCommand("css_guard", "Joins the guard queue")]
    [ConsoleCommand("css_g", "Joins the guard queue")]
    [CommandHelper(0, "", CommandUsage.CLIENT_ONLY)]
    public void Command_Guard(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
            return;
        HandleQueueRequest(player);
    }

    [ConsoleCommand("css_leave", "Leaves the guard queue")]
    [CommandHelper(0, "", CommandUsage.CLIENT_ONLY)]
    public void Command_Leave(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
            return;
        HandleLeaveRequest(player);
    }


    public IEnumerable<CCSPlayerController> Queue
		=> Utilities.GetPlayers()
			.Select(player => (Player: player, State: _state.Get(player)))
			.Where(tuple => tuple.State.InQueue)	//	Exclude not in queue
			.Where(tuple => !tuple.State.IsGuard)	//	Exclude current guards
			.OrderBy(tuple => tuple.State.Position) //	Order by counter value when joined queue
			.Select(tuple => tuple.Player);
}
