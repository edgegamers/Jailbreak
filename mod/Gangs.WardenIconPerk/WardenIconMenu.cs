using Gangs.BaseImpl;
using Jailbreak.Public.Extensions;

namespace Gangs.WardenIconPerk;

public class WardenIconMenu(IServiceProvider provider, WardenIcon data,
  WardenIcon equipped) : AbstractEnumMenu<WardenIcon>(provider, data, equipped,
  "css_wardenicon", "Warden Icon", WardenIconPerk.DESC) {
  override protected int getCost(WardenIcon item) { return item.GetCost(); }

  override protected List<WardenIcon> getValues() {
    return Enum.GetValues<WardenIcon>().ToList();
  }

  override protected string formatItem(WardenIcon item) {
    return $"{item.GetIcon()} ({item.ToString().ToTitleCase()})";
  }
}