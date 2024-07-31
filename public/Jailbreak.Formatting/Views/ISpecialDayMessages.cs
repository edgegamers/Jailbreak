using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Objects;

namespace Jailbreak.Formatting.Views;

public interface ISpecialDayMessages {

  public IView SpecialDayRunning(string name);
  public IView InvalidSpecialDay(string name);
  public IView SpecialDayCooldown(int rounds);
  public IView TooLateForSpecialDay(int maxTime);
}