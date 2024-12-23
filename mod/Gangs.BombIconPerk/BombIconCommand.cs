using Gangs.BaseImpl;
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Gangs.BombIconPerk;

public class BombIconCommand(IServiceProvider provider)
  : AbstractEnumCommand<BombIcon>(provider, BombPerk.STAT_ID, BombIcon.DEFAULT,
    "Bomb Icon", true) {
  override protected void openMenu(PlayerWrapper player, BombIcon data,
    BombIcon equipped) {
    var bombPerkData =
      new BombPerkData { Equipped = equipped, Unlocked = data };
    var menu = new BombIconMenu(Provider, bombPerkData);
    Menus.OpenMenu(player, menu);
  }

  public override string Name => "css_bombicon";

  override protected int getCost(BombIcon item) { return item.GetCost(); }
}