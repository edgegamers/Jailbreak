using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.English.SpecialDay;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;
using Jailbreak.Public.Utils;

namespace Jailbreak.SpecialDay.SpecialDays;

public class HideAndSeekDay(BasePlugin plugin, IServiceProvider provider)
  : ArmoryRestrictedDay(plugin, provider), ISpecialDayMessageProvider {
  public override SDType Type => SDType.HNS;

  private HNSDayMessages msg => (HNSDayMessages)Messages;

  public override SpecialDaySettings Settings => new HNSSettings();

  public override IView ArmoryReminder => msg.StayInArmory;

  public ISpecialDayInstanceMessages Messages => new HNSDayMessages();

  public override void Setup() {
    Timers[5] += () => msg.DamageWarning(5).ToTeamChat(CsTeam.CounterTerrorist);
    Timers[10] += () => {
      foreach (var ct in PlayerUtil.FromTeam(CsTeam.CounterTerrorist)) {
        ct.SetSpeed(1.5f);
        EnableDamage(ct);
      }

      ((ISpecialDayMessageProvider)this).Messages.BeginsIn(25).ToAllChat();
    };
    Timers[25] += () => {
      foreach (var ct in PlayerUtil.FromTeam(CsTeam.CounterTerrorist)) {
        ct.SetSpeed(1.25f);
        EnableDamage(ct);
      }

      Messages.BeginsIn(10).ToAllChat();
    };
    Timers[30] += () => {
      foreach (var ct in PlayerUtil.FromTeam(CsTeam.CounterTerrorist))
        ct.SetSpeed(1.1f);
    };
    Timers[35] += Execute;

    base.Setup();

    foreach (var ct in PlayerUtil.FromTeam(CsTeam.CounterTerrorist))
      ct.SetSpeed(2f);
  }

  public override void Execute() {
    base.Execute();
    foreach (var t in PlayerUtil.FromTeam(CsTeam.Terrorist)) t.SetArmor(100);
    foreach (var ct in PlayerUtil.FromTeam(CsTeam.CounterTerrorist))
      ct.SetSpeed(1);
  }

  public class HNSSettings : SpecialDaySettings {
    public HNSSettings() {
      AllowLastRequests = true;
      TTeleport         = TeleportType.ARMORY;
      CtTeleport        = TeleportType.ARMORY;
    }

    public override int InitialHealth(CCSPlayerController player) {
      return player.GetTeam() == CsTeam.Terrorist ? 250 : 50;
    }

    public override int InitialArmor(CCSPlayerController player) {
      if (player.GetTeam() != CsTeam.Terrorist) return -1;
      return 500;
    }

    public override ISet<string>? AllowedWeapons(CCSPlayerController player) {
      if (player.Team != CsTeam.CounterTerrorist) return null;
      return Tag.PISTOLS.Union(Tag.UTILITY).ToHashSet();
    }
  }
}