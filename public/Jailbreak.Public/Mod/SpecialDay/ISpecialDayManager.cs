using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.SpecialDay.Enums;

namespace Jailbreak.Public.Mod.SpecialDay;

public interface ISpecialDayManager : IPluginBehavior {
  public bool IsSDEnabled { get; set; }
  public SDType? ActiveSD { get; }

  bool InitiateSpecialDay(SDType type);
}