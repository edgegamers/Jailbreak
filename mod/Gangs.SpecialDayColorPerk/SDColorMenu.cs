using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Gangs.BaseImpl;
using Gangs.BaseImpl.Extensions;
using GangsAPI.Data;
using GangsAPI.Extensions;
using GangsAPI.Menu;
using GangsAPI.Services.Commands;
using Jailbreak.Public.Mod.Rainbow;
using Microsoft.Extensions.DependencyInjection;

namespace Gangs.SpecialDayColorPerk;

public class SDColorMenu(IServiceProvider provider, SDColorData data)
  : AbstractEnumMenu<SDColor>(provider, data.Unlocked, data.Equipped,
    "css_sdcolor", "SD Color", SDColorPerk.DESC) {
  override protected int getCost(SDColor item) { return item.GetCost(); }

  override protected List<SDColor> getValues() {
    return Enum.GetValues<SDColor>().ToList();
  }

  override protected string formatItem(SDColor item) {
    if (item == SDColor.RAINBOW) return IRainbowColorizer.RAINBOW;
    return
      $"{item.GetColor().GetChatColor()}{item.ToString().ToTitleCase()}{ChatColors.Grey}";
  }
}