using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Jailbreak.English.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;

namespace Jailbreak.SpecialDay.SpecialDays;

public class FFASpecialDay(BasePlugin plugin, IServiceProvider provider)
  : DelayedStartSpecialDay(plugin, provider, new FfaInstanceMessages(), 25) {
  public override SDType Type => SDType.FFA;

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

  public override void Execute() {
    base.Execute();
    Server.PrintToChatAll("GO!");
  }
}