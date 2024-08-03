using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views.LastRequest;

/// <summary>
///   Last Request Rock Paper Scissors Locale
/// </summary>
public interface ILRRPSLocale : ILRLocale {
  public IView PlayerMadeChoice(CCSPlayerController player);
  public IView BothPlayersMadeChoice();
  public IView Tie();

  public IView Results(CCSPlayerController guard, CCSPlayerController prisoner,
    int guardPick, int prisonerPick);
}