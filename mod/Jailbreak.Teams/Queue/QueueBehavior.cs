using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Generic;
using Jailbreak.Public.Mod.Teams;
using Microsoft.Extensions.Logging;

namespace Jailbreak.Teams.Queue;

public class QueueBehavior : IGuardQueue, IPluginBehavior
{
    private int _counter;
    private readonly ILogger<QueueBehavior> _logger;

    private readonly IRatioNotifications _notifications;
    private readonly IPlayerState<QueueState> _state;
    private BasePlugin _parent;

    public QueueBehavior(IPlayerStateFactory factory, IRatioNotifications notifications, ILogger<QueueBehavior> logger)
    {
        _logger = logger;
        _notifications = notifications;
        _counter = 0;
        _state = factory.Global<QueueState>();
    }

    public bool TryEnterQueue(CCSPlayerController player)
    {
        if (!player.IsReal())
            return false;

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
        if (!player.IsReal())
            return false;

        var state = _state.Get(player);
        state.InQueue = false;
        state.IsGuard = false;

        return true;
    }

    public bool TryPop(int count)
    {
        var queue = Queue.Where(p => p.IsReal()).ToList();

        if (queue.Count <= count)
        {
            _notifications.NOT_ENOUGH_GUARDS.ToAllChat();
            _notifications.PLEASE_JOIN_GUARD_QUEUE.ToAllChat().ToAllCenter();
        }

        _logger.LogInformation("[Queue] Pop requested {@Count} out of {@InQueue}", count, queue.Count);

        for (var i = 0; i < Math.Min(queue.Count, count); i++)
        {
            _logger.LogInformation("[Queue] Popping player {@Name}", queue[i].PlayerName);

            ForceGuard(queue[i]);
        }

        return true;
    }

    public bool TryPush(int count)
    {
        var players = Utilities.GetPlayers()
            .Where(p => p.IsReal() && p.GetTeam() == CsTeam.CounterTerrorist)
            .Shuffle(Random.Shared)
            .ToList();
        _logger.LogInformation("[Queue] Push requested {@Count} out of {@GuardCount}", count, players.Count);

        for (var i = 0; i < Math.Min(count, players.Count); i++)
        {
            var toSwap = players[i];
            _logger.LogInformation("[Queue] Pushing {@Name}", toSwap.PlayerName);
            var state = _state.Get(toSwap);

            state.IsGuard = false;
            toSwap.ChangeTeam(CsTeam.Terrorist);
            toSwap.Respawn();

            TryEnterQueue(toSwap);

            _notifications.YOU_WERE_AUTOBALANCED_PRISONER.ToPlayerCenter(toSwap);
        }

        return true;
    }

    public void ForceGuard(CCSPlayerController player)
    {
        //	Set IsGuard so they won't be swapped back.
        _state.Get(player).IsGuard = true;

        _notifications.YOU_WERE_AUTOBALANCED_GUARD
            .ToPlayerChat(player)
            .ToPlayerCenter(player);

        player.ChangeTeam(CsTeam.CounterTerrorist);
        player.Respawn();
    }

    public int GetQueuePosition(CCSPlayerController player)
    {
        return Queue.ToList()
            .FindIndex(controller => controller.Slot == player.Slot);
    }


    public IEnumerable<CCSPlayerController> Queue
        => Utilities.GetPlayers()
            .Select(player => (Player: player, State: _state.Get(player)))
            .Where(tuple => tuple.State.InQueue) //	Exclude not in queue
            .Where(tuple => !tuple.State.IsGuard) //	Exclude current guards
            .OrderBy(tuple => tuple.State.Position) //	Order by counter value when joined queue
            .Select(tuple => tuple.Player);

    public void Start(BasePlugin parent)
    {
        _parent = parent;
        //	Listen for the player requesting to join a team.
        //	Thanks, destoer!
        parent.AddCommandListener("jointeam", OnRequestToJoinTeam);
    }

    /// <summary>
    ///     Block players from joining the CT team using the "m" menu.
    /// </summary>
    /// <param name="invoked"></param>
    /// <param name="command"></param>
    /// <returns></returns>
    public HookResult OnRequestToJoinTeam(CCSPlayerController? invoked, CommandInfo command)
    {
        if (invoked == null || !invoked.IsReal())
            return HookResult.Continue;

        var state = _state.Get(invoked);

        //	Invalid command? Stop here to be safe.
        if (command.ArgCount < 2)
            return HookResult.Stop;

        if (!state.IsGuard)
            _parent.AddTimer(0.1f, () =>
            {
                if (!invoked.IsReal())
                    return;
                if (invoked.GetTeam() != CsTeam.CounterTerrorist)
                    return;
                _notifications.ATTEMPT_TO_JOIN_FROM_TEAM_MENU
                    .ToPlayerChat(invoked)
                    .ToPlayerCenter(invoked);
                invoked.ChangeTeam(CsTeam.Terrorist);
            });
        if (!int.TryParse(command.ArgByIndex(1), out var team))
            return HookResult.Stop;

        if (Utilities.GetPlayers().Find(c => c.GetTeam() == CsTeam.CounterTerrorist) == null)
            return HookResult.Continue; // If no CTs, let anyone on CT team

        //	Player is attempting to join CT and is not a guard?
        //	If so, stop them!!
        if ((CsTeam)team == CsTeam.CounterTerrorist && !state.IsGuard)
        {
            _notifications.ATTEMPT_TO_JOIN_FROM_TEAM_MENU
                .ToPlayerChat(invoked)
                .ToPlayerCenter(invoked);

            return HookResult.Stop;
        }

        //	All else: A-OK.
        return HookResult.Continue;
    }

    /// <summary>
    ///     Remove guards from the team if they are not a guard in the queue state
    /// </summary>
    /// <param name="ev"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    [GameEventHandler]
    public HookResult OnPlayerSpawn(EventPlayerSpawn ev, GameEventInfo info)
    {
        var player = ev.Userid;
        if (!player.IsReal())
            return HookResult.Continue;

        var state = _state.Get(ev.Userid);

        if (player.GetTeam() == CsTeam.CounterTerrorist && !state.IsGuard)
        {
            _notifications.ATTEMPT_TO_JOIN_FROM_TEAM_MENU
                .ToPlayerChat(player)
                .ToPlayerCenter(player);

            player.ChangeTeam(CsTeam.Terrorist);
            player.Respawn();
        }

        return HookResult.Continue;
    }

    /// <summary>
    ///     Remove guard state if they switch to the terrorist team.
    /// </summary>
    /// <param name="ev"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    [GameEventHandler]
    public HookResult OnPlayerTeam(EventPlayerTeam ev, GameEventInfo info)
    {
        var state = _state.Get(ev.Userid);
        var player = ev.Userid;

        if ((CsTeam)ev.Team != CsTeam.CounterTerrorist && state.IsGuard)
            if (TryExitQueue(player))
                _notifications.LEFT_GUARD
                    .ToPlayerCenter(player)
                    .ToPlayerChat(player);

        return HookResult.Continue;
    }

    private void HandleQueueRequest(CCSPlayerController player)
    {
        if (TryEnterQueue(player))
            _notifications.JOINED_GUARD_QUEUE
                .ToPlayerCenter(player)
                .ToPlayerChat(player);
        else
            player.PrintToCenter("An error occured adding you to the queue.");
    }

    private void HandleLeaveRequest(CCSPlayerController player)
    {
        if (TryExitQueue(player))
            _notifications.LEFT_GUARD
                .ToPlayerCenter(player)
                .ToPlayerChat(player);
        else
            player.PrintToCenter("An error occured removing you from the queue.");
    }

    [ConsoleCommand("css_guard", "Joins the guard queue")]
    [ConsoleCommand("css_g", "Joins the guard queue")]
    [CommandHelper(0, "", CommandUsage.CLIENT_ONLY)]
    public void Command_Guard(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
            return;
        if (player.AuthorizedSteamID == null)
        {
            player.PrintToCenter("Steam has not yet authorized you. Please try again later.");
            return;
        }

        // AdminData? data = AdminManager.GetPlayerAdminData(player.AuthorizedSteamID!);

        // if (data == null || !data.Groups.Contains("#ego/e"))
        // {
        //     player.PrintToCenter("You must be an =(e)= to join the guard queue. Apply at https://edgm.rs/join");
        //     return;
        // }

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
}