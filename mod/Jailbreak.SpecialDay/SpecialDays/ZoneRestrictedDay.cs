﻿using System.Drawing;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Public.Mod.SpecialDay;
using Jailbreak.Public.Mod.Zones;
using Jailbreak.Public.Utils;
using Jailbreak.Zones;

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
    GetZone().Draw(plugin, Color.Firebrick, 55);

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