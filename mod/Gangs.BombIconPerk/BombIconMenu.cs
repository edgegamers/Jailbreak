using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using GangsAPI.Data;
using GangsAPI.Exceptions;
using GangsAPI.Extensions;
using GangsAPI.Services.Commands;
using GangsAPI.Services.Gang;
using GangsAPI.Services.Menu;
using Menu;
using Microsoft.Extensions.DependencyInjection;

namespace Gangs.BombIconPerk;

public class BombIconMenu(IServiceProvider provider, BombPerkData data)
  : AbstractPagedMenu<BombIcon>(provider, NativeSenders.Chat, 7) {
  private readonly ICommandManager commands =
    provider.GetRequiredService<ICommandManager>();

  // Method to sort bomb icons
  private int CompareBombIcons(BombIcon a, BombIcon b) {
    // If the icon is equipped, it should be first
    if (a == data.Equipped) return -1;

    // If icon is unlocked, it should be next
    // If both are unlocked, sort by cost (highest first)
    if (a == data.Unlocked) {
      if (b == data.Equipped) return 1;
      return b == data.Unlocked ? a.GetCost().CompareTo(b.GetCost()) : -1;
    }

    // If both are locked, sort by cost (lowest first)
    if (b == data.Equipped) return 1;
    return b == data.Unlocked ? 1 : a.GetCost().CompareTo(b.GetCost());
  }

  override protected Task<List<BombIcon>> GetItems(PlayerWrapper player) {
    var list = Enum.GetValues<BombIcon>().ToList();
    list.Sort(CompareBombIcons);
    list.Insert(0, 0);
    return Task.FromResult(list);
  }

  override protected Task HandleItemSelection(PlayerWrapper player,
    List<BombIcon> items, int selectedIndex) {
    commands.ProcessCommand(player, CommandCallingContext.Chat,
      "css_bombicon " + items[selectedIndex]);
    return Task.CompletedTask;
  }

  override protected Task<string> FormatItem(PlayerWrapper player, int index,
    BombIcon item) {
    var name = item.ToString().ToTitleCase();
    if (item == 0)
      return Task.FromResult(
        $"{ChatColors.DarkBlue}Gang Perks: {ChatColors.LightBlue}Bomb Icons");
    if (item == data.Equipped)
      return Task.FromResult(
        $"{index}. {ChatColors.DarkRed}{name} {ChatColors.Green}({ChatColors.Lime}Equipped{ChatColors.Green})");
    if (item == data.Unlocked)
      return Task.FromResult(
        $"{index}. {ChatColors.LightRed}{name} {ChatColors.Green}({ChatColors.Grey}Unlocked{ChatColors.Green})");
    return Task.FromResult(
      $"{index}. {ChatColors.Grey}{name} {ChatColors.DarkRed}({ChatColors.LightRed}{item.GetCost()}{ChatColors.DarkRed})");
  }
}