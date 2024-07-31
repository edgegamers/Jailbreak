using Jailbreak.Formatting.Views;

namespace Jailbreak.SpecialDay.SpecialDays;

public interface ISpecialDayMessageProvider {
  public ISpecialDayInstanceMessages Messages { get; }
}