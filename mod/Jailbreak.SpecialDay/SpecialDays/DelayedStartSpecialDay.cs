using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Views;

namespace Jailbreak.SpecialDay.SpecialDays;

public abstract class DelayedStartSpecialDay(BasePlugin plugin,
  IServiceProvider provider, ISpecialDayInstanceMessages messages, float delay)
  : MessagedSpecialDay(plugin, provider, messages) {
  public override void Setup() {
    base.Setup();
    Plugin.AddTimer(delay, Execute);
  }
}