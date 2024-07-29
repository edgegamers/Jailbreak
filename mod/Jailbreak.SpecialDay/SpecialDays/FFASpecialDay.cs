using CounterStrikeSharp.API.Core;
using Jailbreak.English.SpecialDay;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;

namespace Jailbreak.SpecialDay.SpecialDays;

public class FFASpecialDay(BasePlugin plugin, IServiceProvider provider)
  : AbstractSpecialDay(plugin, provider) {
  public override SDType Type => SDType.FFA;
  public override ISpecialDayMessages Messages => new FFAMessages();
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

  public override void Setup() { base.Setup(); }
  public override void Execute() { throw new NotImplementedException(); }
}