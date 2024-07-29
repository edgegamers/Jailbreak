using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views;

public interface IRaceLRMessages {
  public IView EndRaceInstruction { get; }

  public IView RaceStartingMessage(CCSPlayerController prisoner);

  public IView NotInRaceLR();

  public IView NotInPendingState();
}