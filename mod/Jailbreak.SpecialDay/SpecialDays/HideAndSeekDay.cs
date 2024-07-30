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

public class HideAndSeekDay(BasePlugin plugin, IServiceProvider provider)
  : MessagedSpecialDay(plugin, provider, new HNSInstanceMessages()) {
  public override SDType Type => SDType.WARDAY;
  private HNSInstanceMessages msg => (HNSInstanceMessages)Messages;

  public class HNSSettings : SpecialDaySettings {
    public HNSSettings() { AllowLastRequests = true; }

    public override int InitialHealth(CCSPlayerController player) {
      return player.GetTeam() == CsTeam.CounterTerrorist ? 250 : 50;
    }

    public override int InitialArmor(CCSPlayerController player) {
      if (player.GetTeam() != CsTeam.CounterTerrorist) return -1;
      return 500;
    }
  }

  public override SpecialDaySettings? Settings => new HNSSettings();

  public override void Setup() {
    Timers[20] += () => Messages.BeginsIn(10);
    Timers[30] += Execute;

    base.Setup();
    msg.StayInArmory.ToTeamChat(CsTeam.CounterTerrorist);
    foreach (var ct in PlayerUtil.FromTeam(CsTeam.CounterTerrorist))
      ct.SetSpeed(0.2f);
  }

  public override void Execute() {
    base.Execute();

    msg.ReadyOrNot.ToAllChat();
    foreach (var ct in PlayerUtil.FromTeam(CsTeam.CounterTerrorist)) {
      ct.SetSpeed(1.0f);
      ct.SetArmor(100);
    }
  }
}