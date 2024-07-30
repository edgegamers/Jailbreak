using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Public.Mod.Draw;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.SpecialDay.Enums;
using Jailbreak.Public.Utils;

namespace Jailbreak.SpecialDay.SpecialDays;

public abstract class ArmoryRestrictedDay(BasePlugin plugin,
  IServiceProvider provider, CsTeam restrictedTeam = CsTeam.Terrorist)
  : AbstractSpecialDay(plugin, provider) {
  private readonly IList<MovementRestrictor> restrictors =
    new List<MovementRestrictor>();

  public override void Setup() {
    base.Setup();

    ArmoryReminder.ToTeamChat(restrictedTeam);

    var armoryBounds = new ConvexHullBorder(Utilities
     .FindAllEntitiesByDesignerName<SpawnPoint>("info_player_counterterrorist")
     .Where(s => s.AbsOrigin != null)
     .Select(s => s.AbsOrigin!)
     .ToList());

    var points = armoryBounds.GetPoints().ToList();
    for (var i = 0; i < points.Count; i++) {
      var first  = points[i];
      var second = points[(i + 1) % points.Count];
      var line   = new BeamLine(plugin, first, second);
      line.SetWidth(1f);
      line.SetColor(Color.Firebrick);
      line.Draw(55);
    }

    foreach (var t in PlayerUtil.FromTeam(restrictedTeam)) {
      var armoryRestrictor = new BorderMovementRestrictor(plugin, t,
        armoryBounds, onTeleport: () => ArmoryReminder.ToPlayerChat(t));
      restrictors.Add(armoryRestrictor);
    }
  }

  public override void Execute() {
    base.Execute();

    foreach (var restrictor in restrictors) { restrictor.Kill(); }
  }

  public abstract IView ArmoryReminder { get; }
}