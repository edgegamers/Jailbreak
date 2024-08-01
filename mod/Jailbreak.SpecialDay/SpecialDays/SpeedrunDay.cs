using System.Drawing;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.English.SpecialDay;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;
using Jailbreak.Public.Mod.Trail;
using Jailbreak.Public.Utils;
using Jailbreak.Trail;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.SpecialDay.SpecialDays;

public class SpeedrunDay(BasePlugin plugin, IServiceProvider provider)
  : AbstractSpecialDay(plugin, provider), ISpecialDayMessageProvider {
  private IGenericCommandNotifications generics;
  private readonly Random rng = new();
  private Vector? target;
  public override SDType Type => SDType.SPEEDRUN;

  private SpeedrunDayMessages msg => (SpeedrunDayMessages)Messages;

  private ActivePlayerTrail<BeamTrailSegment> bestTrail;

  private IDictionary<int, ActivePlayerTrail<VectorTrailSegment>> activeTrails =
    new Dictionary<int, ActivePlayerTrail<VectorTrailSegment>>();

  public override SpecialDaySettings Settings => new SpeedrunSettings();
  public ISpecialDayInstanceMessages Messages => new SpeedrunDayMessages();

  public override void Setup() {
    generics = provider.GetRequiredService<IGenericCommandNotifications>();

    // Timers[60] 

    var speedrunner = PlayerUtil.GetRandomFromTeam(rng.Next(2) == 0 ?
      CsTeam.Terrorist :
      CsTeam.CounterTerrorist);

    if (speedrunner == null) {
      speedrunner = PlayerUtil.GetAlive().FirstOrDefault();
      if (speedrunner == null) {
        generics.Error("Could not find a valid speedrunner").ToAllChat();
        RoundUtil.SetTimeRemaining(1);
        return;
      }
    }

    Timers[2] += () => msg.RunnerAssigned(speedrunner).ToAllChat();

    base.Setup();

    speedrunner.UnFreeze();
    msg.YouAreRunner(60).ToPlayerChat(speedrunner);
    bestTrail = new ActiveBeamPlayerTrail(plugin, speedrunner);
    speedrunner.SetColor(Color.CornflowerBlue);
  }

  public override HookResult OnEnd(EventRoundEnd @event, GameEventInfo info) {
    var result = base.OnEnd(@event, info);

    bestTrail.Kill();
    foreach (var trail in activeTrails.Values) { trail.Kill(); }

    return result;
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

    public override float FreezeTime(CCSPlayerController player) { return 3; }
  }
}