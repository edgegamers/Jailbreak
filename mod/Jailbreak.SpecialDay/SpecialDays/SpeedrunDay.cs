using System.Drawing;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.English.SpecialDay;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;
using Jailbreak.Public.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.SpecialDay.SpecialDays;

public class SpeedrunDay(BasePlugin plugin, IServiceProvider provider)
  : AbstractSpecialDay(plugin, provider), ISpecialDayMessageProvider {
  private IGenericCommandNotifications generics;
  private readonly Random rng = new();
  private CCSPlayerController? speedrunner;
  private Vector? target;
  public override SDType Type => SDType.SPEEDRUN;

  private SpeedrunDayMessages msg => (SpeedrunDayMessages)Messages;

  public override SpecialDaySettings Settings => new SpeedrunSettings();
  public ISpecialDayInstanceMessages Messages => new SpeedrunDayMessages();

  public override void Setup() {
    generics = provider.GetRequiredService<IGenericCommandNotifications>();

    // Timers[60] 

    base.Setup();
    speedrunner = PlayerUtil.GetRandomFromTeam(rng.Next(2) == 0 ?
      CsTeam.Terrorist :
      CsTeam.CounterTerrorist);

    if (speedrunner == null) {
      generics.Error("Could not find a valid speedrunner").ToAllChat();
      RoundUtil.SetTimeRemaining(1);
      return;
    }

    msg.YouAreRunner(60).ToPlayerChat(speedrunner);
    speedrunner.SetColor(Color.CornflowerBlue);
  }

  public class SpeedrunSettings : SpecialDaySettings {
    public SpeedrunSettings() {
      CtTeleport      = TeleportType.ARMORY_STACKED;
      TTeleport       = TeleportType.ARMORY_STACKED;
      RestrictWeapons = true;
      StripToKnife    = true;
    }

    public override ISet<string>? AllowedWeapons(CCSPlayerController player) {
      // Return empty set to allow no weapons
      return new HashSet<string>();
    }
  }
}