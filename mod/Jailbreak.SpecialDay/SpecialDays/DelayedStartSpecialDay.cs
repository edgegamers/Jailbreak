using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Mod.SpecialDay;

namespace Jailbreak.SpecialDay.SpecialDays;

public abstract class DelayedStartSpecialDay(BasePlugin plugin,
  IServiceProvider provider, float delay)
  : AbstractSpecialDay(plugin, provider) {
  public override void Setup() {
    base.Setup();
    Plugin.AddTimer(delay, Execute);
  }
}