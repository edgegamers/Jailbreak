using Gangs.BaseImpl;
using Jailbreak.Public.Extensions;

namespace Gangs.BombIconPerk;

public class BombIconMenu(IServiceProvider provider, BombPerkData data)
  : AbstractEnumMenu<BombIcon>(provider, data.Unlocked, data.Equipped,
    "css_bombicon", "Bomb Icon", BombPerk.DESC) {
  override protected int getCost(BombIcon item) { return item.GetCost(); }

  override protected List<BombIcon> getValues() {
    return Enum.GetValues<BombIcon>().ToList();
  }

  override protected string formatItem(BombIcon item) {
    return $"{item.ToString().ToTitleCase()}";
  }
}