using CounterStrikeSharp.API.Core;
using Jailbreak.English.SpecialDay;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;

namespace Jailbreak.SpecialDay.SpecialDays;

public class CustomDay(BasePlugin plugin, IServiceProvider provider)
  : AbstractSpecialDay(plugin, provider), ISpecialDayMessageProvider {
  public override SDType Type => SDType.CUSTOM;

  public override SpecialDaySettings Settings
    => new() { StripToKnife = false, AllowLastRequests = true };

  public ISpecialDayInstanceMessages Messages
    => new TeamDayMessages("Custom Day",
      "Listen to the Warden's orders. Anything goes!");
}