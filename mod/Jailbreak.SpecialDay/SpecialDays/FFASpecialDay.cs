using CounterStrikeSharp.API.Core;
using Jailbreak.English.SpecialDay;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Mod.SpecialDay.Enums;

namespace Jailbreak.SpecialDay.SpecialDays;

public class FFASpecialDay(BasePlugin plugin, IServiceProvider provider)
  : DelayedStartSpecialDay(plugin, provider, 25) {
  public override SDType Type => SDType.FFA;
  public override ISpecialDayInstanceMessages Messages => new FfaInstanceMessages();
  public override SpecialDaySettings? Settings => new FFASettings();

  public class FFASettings : SpecialDaySettings {
    private readonly Random rng;

    public FFASettings() {
      AllowLastRequests = false;
      Teleport          = TeleportType.ARMORY;
      rng               = new Random();
      WithFriendlyFire();
    }

    public override float FreezeTime(CCSPlayerController player) {
      return rng.NextSingle() * 5 + 2;
    }
  }
}