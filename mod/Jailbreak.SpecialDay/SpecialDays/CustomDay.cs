using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;

namespace Jailbreak.SpecialDay.SpecialDays;

public class CustomDay(BasePlugin plugin, IServiceProvider provider)
  : AbstractSpecialDay(plugin, provider) {
  public override SDType Type => SDType.CUSTOM;

  public override SpecialDaySettings Settings
    => new() { StripToKnife = false, AllowLastRequests = true };
}