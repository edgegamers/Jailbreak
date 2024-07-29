using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.SpecialDay.Enums;

namespace Jailbreak.Public.Mod.SpecialDay;

public interface ISpecialDayManager : IPluginBehavior {
  public bool IsSDRunning { get; set; }
  public AbstractSpecialDay? CurrentSD { get; }
  public int RoundsSinceLastSD { get; }

  bool InitiateSpecialDay(SDType type);
}