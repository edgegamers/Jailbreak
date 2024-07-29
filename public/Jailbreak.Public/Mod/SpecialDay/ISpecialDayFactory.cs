using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.SpecialDay.Enums;

namespace Jailbreak.Public.Mod.SpecialDay;

public interface ISpecialDayFactory : IPluginBehavior {
  AbstractSpecialDay CreateSpecialDay(SDType type);

  bool IsValidType(SDType type);
}