using CounterStrikeSharp.API.Modules.Utils;
using Gangs.BaseImpl;
using Gangs.BaseImpl.Extensions;
using GangsAPI.Data;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Rainbow;

namespace WardenPaintColorPerk;

public class WardenColorCommand(IServiceProvider provider)
  : AbstractEnumCommand<WardenPaintColor>(provider,
    WardenPaintColorPerk.STAT_ID, WardenPaintColor.DEFAULT, "Paint Color") {
  public override string Name => "css_paint";

  override protected void openMenu(PlayerWrapper player, WardenPaintColor data,
    WardenPaintColor equipped) {
    var menu = new WardenPaintColorMenu(Provider, data, equipped);
    Menus.OpenMenu(player, menu);
  }

  override protected int getCost(WardenPaintColor item) {
    return item.GetCost();
  }

  override protected string formatItem(WardenPaintColor item) {
    if (item == WardenPaintColor.RAINBOW) return IRainbowColorizer.RAINBOW;
    return
      $"{item.GetColor().GetChatColor()}{item.ToString().ToTitleCase()}{ChatColors.Grey}";
  }
}