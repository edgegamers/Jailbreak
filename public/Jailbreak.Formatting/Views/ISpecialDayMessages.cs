using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views;

public interface ISpecialDayMessages {
  public IView SpecialDayStart { get; }
  public IView SpecialDayEnd { get; }
}