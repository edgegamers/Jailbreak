using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views.LastRequest;

/// <summary>
///   Last Request Race Locale
/// </summary>
public interface ILRRaceLocale {
  public IView EndRaceInstruction { get; }

  public IView RaceStartingMessage(CCSPlayerController prisoner);

  public IView NotInRaceLR();

  public IView NotInPendingState();
}