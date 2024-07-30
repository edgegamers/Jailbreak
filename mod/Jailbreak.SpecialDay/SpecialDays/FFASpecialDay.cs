using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Jailbreak.English.SpecialDay;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Public.Mod.SpecialDay.Enums;

namespace Jailbreak.SpecialDay.SpecialDays;

public class FFASpecialDay(BasePlugin plugin, IServiceProvider provider)
  : MessagedSpecialDay(plugin, provider, new FfaInstanceMessages()) {
  public override SDType Type => SDType.FFA;
  public override SpecialDaySettings? Settings => new FFASettings();
  private FfaInstanceMessages msg => (FfaInstanceMessages)Messages;

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

  public override void Setup() {
    Timers[10] += () => msg.DamageEnablingIn(10).ToAllChat();
    Timers[15] += () => msg.DamageEnablingIn(5).ToAllChat();
    Timers[20] += Execute;
    base.Setup();
  }

  public override void Execute() {
    base.Execute();
    msg.Begin.ToAllChat();
  }
}