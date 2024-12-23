using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Gangs.BaseImpl;
using GangsAPI.Data;
using GangsAPI.Menu;
using GangsAPI.Services.Commands;
using Jailbreak.Public.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Gangs.BombIconPerk;

public class BombIconMenu(IServiceProvider provider, BombPerkData data)
  : AbstractEnumMenu<BombIcon>(provider, data.Unlocked, data.Equipped,
    "css_bombicon", "Bomb Icon", new BombPerk(provider).Description) {
  override protected int getCost(BombIcon item) { return item.GetCost(); }

  override protected List<BombIcon> getValues() {
    return Enum.GetValues<BombIcon>().ToList();
  }

  override protected string formatItem(BombIcon item) {
    return $"{item.ToString().ToTitleCase()}";
  }
}