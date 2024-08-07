using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views.SpecialDay;

public interface ISDInstanceLocale : ISDLocale {
  public string Name { get; }
  public string[] Description { get; }

  public IView SpecialDayStart { get; }

  public IView SpecialDayEnd { get; }

  public IView BeginsIn(int seconds);
}