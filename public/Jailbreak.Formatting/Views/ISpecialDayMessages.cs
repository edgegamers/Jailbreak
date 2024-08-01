using Jailbreak.Formatting.Base;

namespace Jailbreak.Formatting.Views;

public interface ISpecialDayMessages {
  public IView SpecialDayRunning(string name);
  public IView InvalidSpecialDay(string name);
  public IView SpecialDayCooldown(int rounds);
  public IView TooLateForSpecialDay(int maxTime);
}