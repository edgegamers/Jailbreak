using CounterStrikeSharp.API.Core;
using Jailbreak.English.SpecialDay;
using Jailbreak.Formatting.Views.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;

namespace Jailbreak.SpecialDay.SpecialDays;

public class BHopDay(BasePlugin plugin, IServiceProvider provider)
  : AbstractSpecialDay(plugin, provider), ISpecialDayMessageProvider {
  public override SDType Type => SDType.CUSTOM;

  public override SpecialDaySettings Settings => new BHopSettings();

  public ISDInstanceLocale Locale
    => new TeamDayLocale("Bunny Hop Day",
      "Auto-Bunny hopping is on, otherwise normal day!");

  public override void Setup() {
    Timers[3] += Execute;
    base.Setup();
  }

  public class BHopSettings : SpecialDaySettings {
    public BHopSettings() {
      StripToKnife      = false;
      AllowLastRequests = true;
      AllowRebels       = true;
      OpenCells         = false;

      ConVarValues["sv_enablebunnyhopping"] = true;
      ConVarValues["sv_autobunnyhopping"]   = true;
    }
  }
}