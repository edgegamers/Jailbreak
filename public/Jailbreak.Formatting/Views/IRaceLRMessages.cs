using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views;

public interface IRaceLRMessages {
  public IView END_RACE_INSTRUCTION { get; }

  public IView RACE_STARTING_MESSAGE(CCSPlayerController prisoner);
}