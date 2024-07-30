using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.English.SpecialDay;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;
using Jailbreak.Public.Utils;

namespace Jailbreak.SpecialDay.SpecialDays;

public class WardaySpecialDay(BasePlugin plugin, IServiceProvider provider)
  : MessagedSpecialDay(plugin, provider,
    new TeamDayMessages("Warday", "CTs pick a room and T's must fight them!")) {
  public override SDType Type => SDType.WARDAY;
  private WardayInstanceMessages msg => (WardayInstanceMessages)Messages;

  public class WardaySettings : SpecialDaySettings {
    public WardaySettings() {
      AllowLastGuard   = true;
      ForceTeleportAll = true;
      Teleport         = TeleportType.ARMORY;
    }

    public override float FreezeTime(CCSPlayerController player) {
      return player.GetTeam() == CsTeam.CounterTerrorist ? 3 : 30;
    }
  }

  public override SpecialDaySettings? Settings => new WardaySettings();

  public override void Setup() {
    Timers[15]  += () => Messages.BeginsIn(15).ToAllChat();
    Timers[15]  += () => Messages.BeginsIn(5).ToAllChat();
    Timers[30]  += Execute;
    Timers[150] += () => msg.ExpandIn(15).ToAllChat();
    Timers[146] += () => {
      msg.ExpandNow.ToAllChat();
      foreach (var ct in PlayerUtil.FromTeam(CsTeam.CounterTerrorist)) {
        ct.SetHealth(100);
        ct.SetArmor(100);
        ct.SetSpeed(1.5f);
      }
    };

    base.Setup();
  }
}