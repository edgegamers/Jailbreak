using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using GangsAPI.Data;
using GangsAPI.Menu;
using GangsAPI.Services.Commands;
using Jailbreak.Public.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Gangs.WardenIconPerk;

public class WardenIconMenu(IServiceProvider provider, WardenIcon data,
  WardenIcon equipped)
  : AbstractPagedMenu<WardenIcon>(provider, NativeSenders.Chat) {
  private readonly ICommandManager commands =
    provider.GetRequiredService<ICommandManager>();

  // Method to sort smoke colors
  private int CompareSmokeColors(WardenIcon a, WardenIcon b) {
    if (a == equipped) return -1;
    if (b == equipped) return 1;
    // If icon is unlocked, it should be next
    // If both are unlocked, sort by cost (highest first)
    if (data.HasFlag(a)) {
      if (data.HasFlag(b)) return a.GetCost().CompareTo(b.GetCost());
      return -1;
    }

    // If both are locked, sort by cost (lowest first)
    return data.HasFlag(b) ? 1 : a.GetCost().CompareTo(b.GetCost());
  }

  override protected Task<List<WardenIcon>> GetItems(PlayerWrapper player) {
    var list = Enum.GetValues<WardenIcon>().ToList();
    list.Sort(CompareSmokeColors);
    list.Insert(0, 0);
    return Task.FromResult(list);
  }

  override protected Task HandleItemSelection(PlayerWrapper player,
    List<WardenIcon> items, int selectedIndex) {
    commands.ProcessCommand(player, CommandCallingContext.Chat,
      "css_wardenicon", items[selectedIndex].ToString());
    Menus.CloseMenu(player);
    return Task.CompletedTask;
  }

  override protected Task<string> FormatItem(PlayerWrapper player, int index,
    WardenIcon item) {
    var name = item.ToString().ToTitleCase();
    if (item == 0)
      return Task.FromResult(
        $" {ChatColors.DarkBlue}Gang Perks: {ChatColors.LightBlue}Warden Icon\n{ChatColors.Grey}{new WardenIconPerk(Provider).Description}");
    if (item == equipped)
      return Task.FromResult(
        $"{index} {ChatColors.LightBlue}{item.GetIcon()} {ChatColors.White}{item.ToString().ToTitleCase()} {ChatColors.Green}({ChatColors.Lime}Equipped{ChatColors.Green})");
    if (data.HasFlag(item))
      return Task.FromResult(
        $"{index}. {ChatColors.DarkBlue}{item.GetIcon()} {ChatColors.Grey}{item.ToString().ToTitleCase()} {ChatColors.Green}({ChatColors.Grey}Unlocked{ChatColors.Green})");
    return Task.FromResult(
      $"{index}. {ChatColors.LightRed}{item.GetIcon()} {ChatColors.Grey}{name} {ChatColors.DarkRed}({ChatColors.LightRed}{item.GetCost()}{ChatColors.DarkRed})");
  }
}