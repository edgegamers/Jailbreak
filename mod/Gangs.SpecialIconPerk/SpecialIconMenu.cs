using Gangs.BaseImpl;
using Jailbreak.Public.Extensions;

namespace Gangs.SpecialIconPerk;

public class SpecialIconMenu(IServiceProvider provider, SpecialIcon data,
  SpecialIcon equipped) : AbstractEnumMenu<SpecialIcon>(provider, data,
  equipped, "css_sticon", "ST Icon",
  new SpecialIconPerk(provider).Description ?? "") {
  override protected int getCost(SpecialIcon item) { return item.GetCost(); }

  override protected List<SpecialIcon> getValues() {
    return Enum.GetValues<SpecialIcon>().ToList();
  }

  override protected string formatItem(SpecialIcon item) {
    return $"{item.GetIcon()} ({item.ToString().ToTitleCase()})";
  }
}