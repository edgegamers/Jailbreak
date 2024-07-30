using System.Drawing;
using System.Runtime.CompilerServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.English.SpecialDay;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.SpecialDay.Enums;
using Jailbreak.Public.Utils;

namespace Jailbreak.SpecialDay.SpecialDays;

public class HideAndSeekDay(BasePlugin plugin, IServiceProvider provider)
  : ArmoryRestrictedDay(plugin, provider), ISpecialDayMessageProvider {
  public override SDType Type => SDType.HNS;

  public ISpecialDayInstanceMessages Messages => new HNSInstanceMessages();

  private HNSInstanceMessages msg => (HNSInstanceMessages)Messages;

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
  }

  public override SpecialDaySettings? Settings => new HNSSettings();

  public override void Setup() {
    Timers[10] += () => {
      foreach (var t in PlayerUtil.FromTeam(CsTeam.Terrorist)) EnableDamage(t);

      ((ISpecialDayMessageProvider)this).Messages.BeginsIn(45).ToAllChat();
    };
    Timers[30] += () => Messages.BeginsIn(25).ToAllChat();
    Timers[45] += () => Messages.BeginsIn(10).ToAllChat();
    Timers[55] += Execute;

    base.Setup();
  }

  public override void Execute() {
    base.Execute();
    msg.ReadyOrNot.ToAllChat();
    foreach (var t in PlayerUtil.FromTeam(CsTeam.Terrorist)) t.SetArmor(100);
  }

  public override IView ArmoryReminder => msg.StayInArmory;
}