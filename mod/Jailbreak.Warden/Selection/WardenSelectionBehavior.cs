using System.Runtime.CompilerServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views.Warden;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Generic;
using Jailbreak.Public.Mod.Warden;
using Jailbreak.Public.Utils;
using Microsoft.Extensions.Logging;

namespace Jailbreak.Warden.Selection;

/// <summary>
///   Behavior responsible for choosing the next warden
/// </summary>
public class
  WardenSelectionBehavior : IPluginBehavior, IWardenSelectionService {
  private readonly ICoroutines coroutines;

  private readonly IPlayerState<QueueFavorState> favor;

  private readonly HashSet<int> guaranteedWarden = [];

  private readonly IWardenLocale locale;

  private readonly ILogger<WardenSelectionBehavior> logger;

  /// <summary>
  ///   A state dict that handles the player's current queue
  ///   enrollment and favor. Uses the Round reset mode.
  /// </summary>
  private readonly IPlayerState<QueueState> queue;

  private readonly IWardenService warden;

  /// <summary>
  ///   Whether or not to use the queue.
  ///   When true, the queue should be skipped and turn to first-come-first-serve.
  /// </summary>
  private bool queueInactive;

  public WardenSelectionBehavior(IPlayerStateFactory factory,
    IWardenService warden, IWardenLocale locale,
    ILogger<WardenSelectionBehavior> logger, ICoroutines coroutines) {
    this.warden     = warden;
    this.locale     = locale;
    this.logger     = logger;
    this.coroutines = coroutines;
    queue           = factory.Round<QueueState>();
    favor           = factory.Global<QueueFavorState>();

    queueInactive = true;
  }

  public void Start(BasePlugin basePlugin) { }

  public void Dispose() { }

  public void SetGuaranteedWarden(CCSPlayerController player) {
    guaranteedWarden.Add(player.UserId ?? -1);
  }

  public bool TryEnter(CCSPlayerController player) {
    if (!canEnterQueue(player)) return false;

    if (guaranteedWarden.Contains(player.UserId ?? -1)) {
      warden.TrySetWarden(player);
      return true;
    }

    queue.Get(player).InQueue = true;
    return true;
  }

  public bool TryExit(CCSPlayerController player) {
    if (!canEnterQueue(player)) return false;

    queue.Get(player).InQueue = false;
    return true;
  }

  public bool InQueue(CCSPlayerController player) {
    return queue.Get(player).InQueue;
  }

  public bool Active => !queueInactive;

  [GameEventHandler]
  public HookResult OnRoundStart(EventRoundStart ev, GameEventInfo info) {
    if (RoundUtil.IsWarmup()) return HookResult.Continue;
    //	Enable the warden queue
    queueInactive = false;

    locale.PickingShortly.ToAllChat();

    //	Start a timer to pick the warden in 7 seconds
    ScheduleChooseWarden();

    return HookResult.Continue;
  }

  [MethodImpl(MethodImplOptions.NoOptimization)]
  public void ScheduleChooseWarden(float time = 7.0f) {
    coroutines.Round(OnChooseWarden, time);
  }

  /// <summary>
  ///   Timer callback that states it's time to choose the warden.
  /// </summary>
  protected void OnChooseWarden() {
    if (warden.HasWarden) return;
    var eligible = Utilities.GetPlayers()
     .Where(player => player.PawnIsAlive)
     .Where(player => player.Team == CsTeam.CounterTerrorist)
     .Where(player => queue.Get(player).InQueue)
     .ToList();

    logger.LogTrace("[WardenSelectionBehavior] Picking warden from {@Eligible}",
      eligible);

    if (eligible.Count == 0) {
      locale.NoWardens.ToAllChat();
      queueInactive = true;

      return;
    }

    var favors = eligible.ToDictionary(player => player,
      player => favor.Get(player));
    var tickets = favors.Sum(f => f.Value.GetTickets());
    var chosen  = Random.Shared.Next(tickets);

    logger.LogTrace("[Warden Raffle] Picking {@Chosen} out of {@Tickets}",
      chosen, tickets);

    var pointer = 0;
    foreach (var (player, f) in favors) {
      var thisTickets = f.GetTickets();
      logger.LogTrace("[Warden Raffle] {@Pointer} -> {@End}: #{@Slot} {@Name}",
        pointer, pointer + thisTickets, player.Slot, player.PlayerName);

      //	If winning ticket belongs to this player, assign them as warden.
      if (pointer <= chosen && chosen < pointer + thisTickets) {
        warden.TrySetWarden(player);
        f.RoundsWithoutWarden = 0;
      } else { f.RoundsWithoutWarden++; }

      pointer += thisTickets;
    }

    //	Disable the warden raffle for future wardens
    //	(eg in the event of warden death)
    queueInactive = true;
  }

  private bool canEnterQueue(CCSPlayerController player) {
    if (player.Team != CsTeam.CounterTerrorist) return false;

    if (!player.PawnIsAlive) return false;

    //	Cannot enter queue if queue is not running
    if (queueInactive) return false;

    //	Edge case: Is there already a warden?
    if (warden.HasWarden) return false;

    return true;
  }
}