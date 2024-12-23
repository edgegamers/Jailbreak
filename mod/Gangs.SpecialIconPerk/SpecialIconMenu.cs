using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Gangs.BaseImpl;
using Gangs.WardenIconPerk;
using GangsAPI.Data;
using GangsAPI.Menu;
using GangsAPI.Services.Commands;
using Jailbreak.Public.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Gangs.SpecialIconPerk;

public class SpecialIconMenu(IServiceProvider provider, SpecialIcon data,
  SpecialIcon equipped) : AbstractEnumMenu<SpecialIcon>(provider, data, equipped,
  "css_sticon", "ST Icon",
  new SpecialIconPerk(provider).Description ?? "") {
  override protected int getCost(SpecialIcon item) { return item.GetCost(); }

  override protected List<SpecialIcon> getValues() {
    return Enum.GetValues<SpecialIcon>().ToList();
  }

  override protected string formatItem(SpecialIcon item) {
    return $"{item.GetIcon()} ({item.ToString().ToTitleCase()})";
  }
}