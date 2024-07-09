using System.Runtime.CompilerServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Generic;
using Jailbreak.Public.Mod.Warden;
using Microsoft.Extensions.Logging;

namespace Jailbreak.Warden.Selection;

/// <summary>
///   Behavior responsible for choosing the next warden
/// </summary>
public class
  WardenSelectionBehavior : IPluginBehavior, IWardenSelectionService {
  private readonly ICoroutines _coroutines;

  private readonly IPlayerState<QueueFavorState> _favor;

  private readonly ILogger<WardenSelectionBehavior> _logger;

  private readonly IWardenNotifications _notifications;

  /// <summary>
  ///   A state dict that handles the player's current queue
  ///   enrollment and favor. Uses the Round reset mode.
  /// </summary>
  private readonly IPlayerState<QueueState> _queue;

  private readonly IWardenService _warden;

  /// <summary>
  ///   Whether or not to use the queue.
  ///   When true, the queue should be skipped and turn to first-come-first-serve.
  /// </summary>
  private bool _queueInactive;

  public WardenSelectionBehavior(IPlayerStateFactory factory,
    IWardenService warden, IWardenNotifications notifications,
    ILogger<WardenSelectionBehavior> logger, ICoroutines coroutines) {
    _warden        = warden;
    _notifications = notifications;
    _logger        = logger;
    _coroutines    = coroutines;
    _queue         = factory.Round<QueueState>();
    _favor         = factory.Global<QueueFavorState>();

    _queueInactive = true;
  }

  public void Start(BasePlugin parent) { }

  public void Dispose() { }

  public bool TryEnter(CCSPlayerController player) {
    if (!CanEnterQueue(player)) return false;

    _queue.Get(player).InQueue = true;
    return true;
  }

  public bool TryExit(CCSPlayerController player) {
    if (!CanEnterQueue(player)) return false;

    _queue.Get(player).InQueue = false;
    return true;
  }

  public bool InQueue(CCSPlayerController player) {
    return _queue.Get(player).InQueue;
  }

  public bool Active => !_queueInactive;

  [GameEventHandler]
  public HookResult OnRoundStart(EventRoundStart ev, GameEventInfo info) {
    //	Enable the warden queue
    _queueInactive = false;

    _notifications.PICKING_SHORTLY.ToAllChat();

    //	Start a timer to pick the warden in 7 seconds
    ScheduleChooseWarden();

    return HookResult.Continue;
  }

  [MethodImpl(MethodImplOptions.NoOptimization)]
  public void ScheduleChooseWarden(float time = 7.0f) {
    _coroutines.Round(OnChooseWarden, time);
  }

  /// <summary>
  ///   Timer callback that states it's time to choose the warden.
  /// </summary>
  protected void OnChooseWarden() {
    var eligible = Utilities.GetPlayers()
     .Where(player => player.PawnIsAlive)
     .Where(player => player.GetTeam() == CsTeam.CounterTerrorist)
     .Where(player => _queue.Get(player).InQueue)
     .ToList();

    _logger.LogTrace(
      "[WardenSelectionBehavior] Picking warden from {@Eligible}", eligible);

    if (eligible.Count == 0) {
      _notifications.NO_WARDENS.ToAllChat();
      _queueInactive = true;

      return;
    }

    var favors = eligible.ToDictionary(player => player,
      player => _favor.Get(player));
    var tickets = favors.Sum(favor => favor.Value.GetTickets());
    var chosen  = Random.Shared.Next(tickets);

    _logger.LogTrace("[Warden Raffle] Picking {@Chosen} out of {@Tickets}",
      chosen, tickets);

    var pointer = 0;
    foreach (var (player, favor) in favors) {
      var thisTickets = favor.GetTickets();
      _logger.LogTrace("[Warden Raffle] {@Pointer} -> {@End}: #{@Slot} {@Name}",
        pointer, pointer + thisTickets, player.Slot, player.PlayerName);

      //	If winning ticket belongs to this player, assign them as warden.
      if (pointer <= chosen && chosen < pointer + thisTickets) {
        _warden.TrySetWarden(player);
        favor.RoundsWithoutWarden = 0;
      } else { favor.RoundsWithoutWarden++; }

      pointer += thisTickets;
    }

    //	Disable the warden raffle for future wardens
    //	(eg in the event of warden death)
    _queueInactive = true;
  }

  private bool CanEnterQueue(CCSPlayerController player) {
    if (player.GetTeam() != CsTeam.CounterTerrorist) return false;

    if (!player.PawnIsAlive) return false;

    //	Cannot enter queue if queue is not running
    if (_queueInactive) return false;

    //	Edge case: Is there already a warden?
    if (_warden.HasWarden) return false;

    return true;
  }
}