using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.English.SpecialDay;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views.SpecialDay;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;
using Jailbreak.Public.Utils;

namespace Jailbreak.SpecialDay.SpecialDays;

public class WardayDay(BasePlugin plugin, IServiceProvider provider)
  : AbstractArmoryRestrictedDay(plugin, provider), ISpecialDayMessageProvider {
  public override SDType Type => SDType.WARDAY;
  private WardayInstanceLocale msg => (WardayInstanceLocale)Locale;

  public override SpecialDaySettings Settings => new WardaySettings();
  public ISDInstanceLocale Locale => new WardayInstanceLocale();

  public override void Setup() {
    Timers[20]  += () => Locale.BeginsIn(30).ToAllChat();
    Timers[35]  += () => Locale.BeginsIn(15).ToAllChat();
    Timers[45]  += () => Locale.BeginsIn(5).ToAllChat();
    Timers[50]  += Execute;
    Timers[120] += () => msg.ExpandIn(30).ToAllChat();
    Timers[150] += () => {
      msg.ExpandNow.ToAllChat();
      foreach (var ct in PlayerUtil.FromTeam(CsTeam.CounterTerrorist)) {
        ct.SetHealth(200);
        ct.SetArmor(300);
        ct.SetSpeed(1.5f);
      }

      foreach (var t in PlayerUtil.FromTeam(CsTeam.Terrorist)) t.SetArmor(0);
    };

    base.Setup();
  }

  public class WardaySettings : SpecialDaySettings {
    public WardaySettings() {
      AllowLastGuard = true;
      TTeleport      = TeleportType.ARMORY;
      CtTeleport     = TeleportType.ARMORY;
    }

    public override float FreezeTime(CCSPlayerController player) {
      return player.Team == CsTeam.CounterTerrorist ? 3 : 5;
    }
  }
}