using CounterStrikeSharp.API.Modules.Utils;
using Gangs.BaseImpl;
using Gangs.BaseImpl.Extensions;
using Jailbreak.Public.Mod.Rainbow;

namespace Gangs.LastRequestColorPerk;

public class LRColorMenu(IServiceProvider provider, LRColor data,
  LRColor equipped) : AbstractEnumMenu<LRColor>(provider, data, equipped,
  "css_lrcolor", "LR Color", LRColorPerk.DESC) {
  override protected int getCost(LRColor item) { return item.GetCost(); }

  override protected List<LRColor> getValues() {
    return Enum.GetValues<LRColor>().ToList();
  }

  override protected string formatItem(LRColor item) {
    var result = item.GetColor().GetChatColor().ToString();
    if (item == LRColor.RAINBOW) return IRainbowColorizer.RAINBOW;
    if (item.GetColor() == null) return result + "Random";
    return $"{result}{item.GetColor()!.Value.Name}{ChatColors.Grey}";
  }
}