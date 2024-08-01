using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views;

public interface ISpecialDayInstanceMessages {
  public string Name { get; }
  public string[] Description { get; }

  public IView SpecialDayStart { get; }

  public IView SpecialDayEnd { get; }

  public IView BeginsIn(int seconds);
}