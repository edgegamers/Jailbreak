using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.English.SpecialDay;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;
using Jailbreak.Public.Utils;

namespace Jailbreak.SpecialDay.SpecialDays;

public class WardaySpecialDay(BasePlugin plugin, IServiceProvider provider)
  : AbstractSpecialDay(plugin, provider), ISpecialDayMessageProvider {
  public override SDType Type => SDType.WARDAY;
  private WardayInstanceMessages msg => (WardayInstanceMessages)Messages;

  public override SpecialDaySettings Settings => new WardaySettings();
  public ISpecialDayInstanceMessages Messages => new WardayInstanceMessages();

  public override void Setup() {
    Timers[15]  += () => Messages.BeginsIn(15).ToAllChat();
    Timers[30]  += () => Messages.BeginsIn(5).ToAllChat();
    Timers[35]  += Execute;
    Timers[150] += () => msg.ExpandIn(15).ToAllChat();
    Timers[165] += () => {
      msg.ExpandNow.ToAllChat();
      foreach (var ct in PlayerUtil.FromTeam(CsTeam.CounterTerrorist)) {
        ct.SetHealth(100);
        ct.SetArmor(100);
        ct.SetSpeed(1.5f);
      }
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
      return player.GetTeam() == CsTeam.CounterTerrorist ? 3 : 30;
    }
  }
}