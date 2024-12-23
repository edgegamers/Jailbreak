using Gangs.BaseImpl;
using Gangs.BaseImpl.Extensions;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Rainbow;

namespace WardenPaintColorPerk;

public class WardenPaintColorMenu(IServiceProvider provider,
  WardenPaintColor data, WardenPaintColor equipped)
  : AbstractEnumMenu<WardenPaintColor>(provider, data, equipped, "css_paint",
    "Paint Color", WardenPaintColorPerk.DESC) {
  override protected int getCost(WardenPaintColor item) {
    return item.GetCost();
  }

  override protected List<WardenPaintColor> getValues() {
    return Enum.GetValues<WardenPaintColor>().ToList();
  }

  override protected string formatItem(WardenPaintColor item) {
    if (item == WardenPaintColor.RAINBOW) return IRainbowColorizer.RAINBOW;
    return $"{item.GetColor().GetChatColor()}{item.ToString().ToTitleCase()}";
  }
}