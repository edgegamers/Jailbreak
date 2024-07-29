using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Mod.SpecialDay;

namespace Jailbreak.SpecialDay.SpecialDays;

public abstract class MessagedSpecialDay(BasePlugin plugin,
  IServiceProvider provider, ISpecialDayInstanceMessages messages)
  : AbstractSpecialDay(plugin, provider) {
  public ISpecialDayInstanceMessages Messages => messages;
}