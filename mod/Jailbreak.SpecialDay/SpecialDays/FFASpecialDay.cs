using CounterStrikeSharp.API.Core;
using Jailbreak.English.SpecialDay;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;

namespace Jailbreak.SpecialDay.SpecialDays;

public class FFASpecialDay(BasePlugin plugin, IServiceProvider provider)
  : AbstractSpecialDay(plugin, provider), ISpecialDayMessageProvider {
  public override SDType Type => SDType.FFA;
  public override SpecialDaySettings Settings => new FFASettings();

  public virtual ISpecialDayInstanceMessages Messages
    => new SoloDayMessages("Free for All",
      "Everyone for themselves! No camping, actively pursue!");

  public override void Setup() {
    Timers[10] += () => Messages.BeginsIn(10).ToAllChat();
    Timers[15] += () => Messages.BeginsIn(5).ToAllChat();
    Timers[20] += Execute;
    base.Setup();
  }

  public override void Execute() {
    base.Execute();
    Messages.BeginsIn(0).ToAllChat();
  }

  public class FFASettings : SpecialDaySettings {
    private readonly Random rng;

    public FFASettings() {
      CtTeleport = TeleportType.ARMORY;
      TTeleport  = TeleportType.ARMORY;
      rng        = new Random();
      WithFriendlyFire();
    }

    public override float FreezeTime(CCSPlayerController player) {
      return rng.NextSingle() * 5 + 2;
    }
  }
}