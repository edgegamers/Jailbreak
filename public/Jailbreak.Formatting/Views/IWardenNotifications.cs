﻿using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Base;

// ReSharper disable InconsistentNaming

namespace Jailbreak.Formatting.Views;

public interface IWardenNotifications {
  public IView PickingShortly { get; }
  public IView NoWardens { get; }
  public IView WardenLeft { get; }
  public IView WardenDied { get; }
  public IView BecomeNextWarden { get; }
  public IView JoinRaffle { get; }
  public IView LeaveRaffle { get; }
  public IView NotWarden { get; }
  public IView FireCommandFailed { get; }

  /// <summary>
  ///   Create a view for when the specified player passes warden
  /// </summary>
  /// <param name="player"></param>
  /// <returns></returns>
  public IView PassWarden(CCSPlayerController player);

  /// <summary>
  ///   Create a view for when this player becomes a new warden
  /// </summary>
  /// <param name="player"></param>
  /// <returns></returns>
  public IView NewWarden(CCSPlayerController player);

  /// <summary>
  ///   Format a response to a request about the current warden.
  ///   When player is null, instead respond stating that there is no warden.
  /// </summary>
  /// <param name="player"></param>
  /// <returns></returns>
  public IView CurrentWarden(CCSPlayerController? player);
  
  public IView CannotWardenDuringWarmup { get; }

  public IView FireCommandSuccess(CCSPlayerController player);

  public IView FireWarden(CCSPlayerController player);

  public IView FireWarden(CCSPlayerController player,
    CCSPlayerController admin);
}