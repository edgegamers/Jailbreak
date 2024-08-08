using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.English.SpecialDay;
using Jailbreak.Formatting.Base;
using Jailbreak.Public.Mod.Zones;
using Jailbreak.Zones;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.SpecialDay.SpecialDays;

public abstract class AbstractArmoryRestrictedDay : AbstractZoneRestrictedDay {
  private readonly IServiceProvider provider;

  protected AbstractArmoryRestrictedDay(BasePlugin plugin,
    IServiceProvider provider,
    CsTeam restrictedTeam = CsTeam.Terrorist) : base(plugin, provider,
    restrictedTeam) {
    this.provider = provider;
  }

  public override IView ZoneReminder => ArmoryReminder;

  public virtual IView ArmoryReminder
    => this is ISpecialDayMessageProvider messaged ?
      new SimpleView {
        SDLocale.PREFIX, $"Today is {messaged.Locale.Name}, so stay in armory!"
      } :
      new SimpleView { SDLocale.PREFIX, "Stay in armory!" };


  override protected IZone GetZone() {
    var manager = provider.GetRequiredService<IZoneManager>();
    var zones   = manager.GetZones(Server.MapName, ZoneType.ARMORY).GetAwaiter().GetResult();
    if (zones.Count > 0) return new MultiZoneWrapper(zones);

    var bounds = new DistanceZone(
      Utilities
       .FindAllEntitiesByDesignerName<
          SpawnPoint>("info_player_counterterrorist")
       .Where(s => s.AbsOrigin != null)
       .Select(s => s.AbsOrigin!)
       .ToList(), DistanceZone.WIDTH_MEDIUM_ROOM);

    return bounds;
  }
}