using Jailbreak.Formatting.Views;
using Jailbreak.Formatting.Views.SpecialDay;

namespace Jailbreak.SpecialDay.SpecialDays;

public interface ISpecialDayMessageProvider {
  public ISDInstanceLocale Locale { get; }
}