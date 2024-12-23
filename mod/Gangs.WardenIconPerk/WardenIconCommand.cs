using Gangs.BaseImpl;
using GangsAPI.Data;

namespace Gangs.WardenIconPerk;

public class WardenIconCommand(IServiceProvider provider)
  : AbstractEnumCommand<WardenIcon>(provider, WardenIconPerk.STAT_ID,
    WardenIcon.DEFAULT, "Warden Icon", false) {
  public override string Name => "css_wardenicon";

  override protected void openMenu(PlayerWrapper player, WardenIcon data,
    WardenIcon equipped) {
    var menu = new WardenIconMenu(Provider, data, equipped);
    Menus.OpenMenu(player, menu);
  }

  override protected int getCost(WardenIcon item) { return item.GetCost(); }
}