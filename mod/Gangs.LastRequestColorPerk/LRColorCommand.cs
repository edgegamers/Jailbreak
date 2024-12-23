using CounterStrikeSharp.API.Modules.Utils;
using Gangs.BaseImpl;
using Gangs.BaseImpl.Extensions;
using GangsAPI;
using GangsAPI.Data;
using GangsAPI.Data.Command;
using GangsAPI.Exceptions;
using GangsAPI.Extensions;
using GangsAPI.Perks;
using GangsAPI.Permissions;
using GangsAPI.Services;
using GangsAPI.Services.Commands;
using GangsAPI.Services.Gang;
using GangsAPI.Services.Menu;
using GangsAPI.Services.Player;
using Jailbreak.Public.Mod.Rainbow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Gangs.LastRequestColorPerk;

public class LRColorCommand(IServiceProvider provider)
  : AbstractEnumCommand<LRColor>(provider, LRColorPerk.STAT_ID, LRColor.DEFAULT,
    "LR Color", false) {
  override protected void openMenu(PlayerWrapper player, LRColor data,
    LRColor equipped) {
    var menu = new LRColorMenu(Provider, data, equipped);
    Menus.OpenMenu(player, menu);
  }

  public override string Name => "css_lrcolor";

  override protected int getCost(LRColor item) { return item.GetCost(); }

  override protected string formatItem(LRColor item) {
    var result = item.GetColor().GetChatColor().ToString();
    if (item == LRColor.RAINBOW) return IRainbowColorizer.RAINBOW;
    if (item.GetColor() == null) return result + "Random";
    return $"{result}{item.GetColor()!.Value.Name}{ChatColors.Grey}";
  }
}