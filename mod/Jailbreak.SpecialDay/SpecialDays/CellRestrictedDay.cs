using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.SpecialDay.SpecialDays;

public abstract class CellRestrictedDay(BasePlugin plugin,
  IServiceProvider provider, CsTeam restrictedTeam = CsTeam.Terrorist)
  : ZoneRestrictedDay(plugin, provider, restrictedTeam) {
  public override IView ZoneReminder => ArmoryReminder;

  public virtual IView ArmoryReminder
    => this is ISpecialDayMessageProvider messaged ?
      new SimpleView {
        ISpecialDayMessages.PREFIX,
        $"Today is {messaged.Messages.Name}, so stay in cells!"
      } :
      new SimpleView { ISpecialDayMessages.PREFIX, "Stay in cells!" };

  override protected IZone GetZone() {
    var manager = provider.GetRequiredService<IZoneManager>();
    var zones   = manager.GetZones(ZoneType.CELL).GetAwaiter().GetResult();
    if (zones.Count > 0) return new MultiZoneWrapper(zones);

    var bounds = new ConvexHullZone(Utilities
     .FindAllEntitiesByDesignerName<SpawnPoint>("info_player_terrorist")
     .Where(s => s.AbsOrigin != null)
     .Select(s => s.AbsOrigin!)
     .ToList());

    return bounds;
  }
}