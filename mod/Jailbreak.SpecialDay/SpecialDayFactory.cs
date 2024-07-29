using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;
using Jailbreak.SpecialDay.SpecialDays;

namespace Jailbreak.SpecialDay;

public class SpecialDayFactory(BasePlugin plugin, IServiceProvider provider)
  : ISpecialDayFactory {
  public AbstractSpecialDay CreateSpecialDay(SDType type) {
    return type switch {
      SDType.FFA => new FFASpecialDay(plugin, provider),
      _          => throw new NotImplementedException()
    };
  }

  public bool IsValidType(SDType type) {
    return type switch {
      SDType.FFA => true,
      _          => false
    };
  }
}