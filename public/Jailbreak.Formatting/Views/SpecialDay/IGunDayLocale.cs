using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views.SpecialDay;

public interface IGunDayLocale : ISDInstanceLocale {
  IView DemotedDueToSuicide { get; }
  IView DemotedDueToKnife { get; }

  IView PromotedTo(string weapon, int weaponsLeft);
  IView PlayerOnLastPromotion(CCSPlayerController player);
  IView PlayerWon(CCSPlayerController player);
}