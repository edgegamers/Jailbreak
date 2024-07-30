using System.Drawing;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Utils;

namespace Jailbreak.SpecialDay.SpecialDays;

public abstract class ZoneRestrictedDay(BasePlugin plugin,
  IServiceProvider provider, CsTeam restrictedTeam = CsTeam.Terrorist)
  : AbstractSpecialDay(plugin, provider) {
  protected readonly IList<MovementRestrictor> Restrictors =
    new List<MovementRestrictor>();


  public abstract IView ZoneReminder { get; }

  abstract protected IZone GetZone();

  public override void Setup() {
    base.Setup();

    ZoneReminder.ToTeamChat(restrictedTeam);

    var points = GetZone().GetPoints().ToList();
    for (var i = 0; i < points.Count; i++) {
      var first  = points[i];
      var second = points[(i + 1) % points.Count];
      var line   = new BeamLine(plugin, first, second);
      line.SetWidth(1f);
      line.SetColor(Color.Firebrick);
      line.Draw(55);
    }

    foreach (var t in PlayerUtil.FromTeam(restrictedTeam)) {
      var zoneRestrictor = new ZoneMovementRestrictor(plugin, t, GetZone(),
        onTeleport: () => ZoneReminder.ToPlayerChat(t));
      Restrictors.Add(zoneRestrictor);
    }
  }

  public override void Execute() {
    base.Execute();

    foreach (var restrictor in Restrictors) restrictor.Kill();
  }
}