using System.Drawing;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.Zones;
using Jailbreak.Public.Utils;
using Jailbreak.Zones;

namespace Jailbreak.SpecialDay.SpecialDays;

public abstract class AbstractZoneRestrictedDay : AbstractSpecialDay {
  protected CsTeam RestrictedTeam;

  protected readonly IList<MovementRestrictor> Restrictors =
    new List<MovementRestrictor>();

  protected AbstractZoneRestrictedDay(BasePlugin plugin,
    IServiceProvider provider,
    CsTeam restrictedTeam = CsTeam.Terrorist) : base(plugin, provider) {
    RestrictedTeam = restrictedTeam;
  }

  public abstract IView ZoneReminder { get; }

  abstract protected IZone GetZone();

  public override void Setup() {
    base.Setup();

    ZoneReminder.ToTeamChat(RestrictedTeam);
    GetZone().Draw(Plugin, Color.Firebrick, 55);

    foreach (var t in PlayerUtil.FromTeam(RestrictedTeam)) {
      var zoneRestrictor = new ZoneMovementRestrictor(Plugin, t, GetZone(),
        DistanceZone.WIDTH_CELL, () => ZoneReminder.ToChat(t));
      Restrictors.Add(zoneRestrictor);
    }
  }

  public override void Execute() {
    base.Execute();
    if (this is ISpecialDayMessageProvider messaged)
      messaged.Locale.BeginsIn(0).ToAllChat();

    foreach (var restrictor in Restrictors) restrictor.Kill();
    Restrictors.Clear();
  }

  override protected HookResult
    OnEnd(EventRoundEnd @event, GameEventInfo info) {
    var result = base.OnEnd(@event, info);
    foreach (var restrictor in Restrictors) restrictor.Kill();
    Restrictors.Clear();
    return result;
  }
}