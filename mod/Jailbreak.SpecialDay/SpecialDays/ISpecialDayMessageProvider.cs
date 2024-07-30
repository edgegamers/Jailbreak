using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Mod.SpecialDay;

namespace Jailbreak.SpecialDay.SpecialDays;

public interface ISpecialDayMessageProvider {
  public ISpecialDayInstanceMessages Messages { get; }
}