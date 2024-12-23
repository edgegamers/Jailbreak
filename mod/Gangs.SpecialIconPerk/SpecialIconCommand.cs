using Gangs.BaseImpl;
using GangsAPI.Data;
using Jailbreak.Public.Extensions;

namespace Gangs.SpecialIconPerk;

public class SpecialIconCommand(IServiceProvider provider)
  : AbstractEnumCommand<SpecialIcon>(provider, SpecialIconPerk.STAT_ID,
    SpecialIcon.DEFAULT, "ST Icon", false) {
  override protected void openMenu(PlayerWrapper player, SpecialIcon data,
    SpecialIcon equipped) {
    var menu = new SpecialIconMenu(Provider, data, equipped);
    Menus.OpenMenu(player, menu);
  }

  public override string Name => "css_sticon";

  override protected int getCost(SpecialIcon item) { return item.GetCost(); }

  override protected string formatItem(SpecialIcon item) {
    return $"{item.GetIcon()} ({item.ToString().ToTitleCase()})";
  }
}