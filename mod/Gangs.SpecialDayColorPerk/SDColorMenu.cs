using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Gangs.BaseImpl.Extensions;
using GangsAPI.Data;
using GangsAPI.Extensions;
using GangsAPI.Menu;
using GangsAPI.Services.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Gangs.SpecialDayColorPerk;

public class SDColorMenu(IServiceProvider provider, SDColorData data)
  : AbstractPagedMenu<SDColor>(provider, NativeSenders.Chat) {
  private readonly ICommandManager commands =
    provider.GetRequiredService<ICommandManager>();

  // Method to sort smoke colors
  private int CompareSDColors(SDColor a, SDColor b) {
    // If the icon is equipped, it should be first
    if (a == data.Equipped) return -1;
    if (b == data.Equipped) return 1;

    // If icon is unlocked, it should be next
    // If both are unlocked, sort by cost (highest first)
    if (data.Unlocked.HasFlag(a)) {
      if (data.Unlocked.HasFlag(b)) return a.GetCost().CompareTo(b.GetCost());
      return -1;
    }

    // If both are locked, sort by cost (lowest first)
    if (data.Unlocked.HasFlag(b)) return 1;
    return a.GetCost().CompareTo(b.GetCost());
  }


  override protected Task<List<SDColor>> GetItems(PlayerWrapper player) {
    var list = Enum.GetValues<SDColor>().ToList();
    list.Sort(CompareSDColors);
    list.Insert(0, 0);
    return Task.FromResult(list);
  }

  override protected Task HandleItemSelection(PlayerWrapper player,
    List<SDColor> items, int selectedIndex) {
    commands.ProcessCommand(player, CommandCallingContext.Chat, "css_sdcolor",
      items[selectedIndex].ToString());
    Menus.CloseMenu(player);
    return Task.CompletedTask;
  }

  override protected Task<string> FormatItem(PlayerWrapper player, int index,
    SDColor item) {
    var name = item.ToString().ToTitleCase();
    if (item == 0)
      return Task.FromResult(
        $" {ChatColors.DarkBlue}Gang Perks: {ChatColors.LightBlue}Special Day Colors");
    if (item == data.Equipped)
      return Task.FromResult(
        $"{index}. {item.GetColor().GetChatColor()}{name} {ChatColors.Green}({ChatColors.Lime}Equipped{ChatColors.Green})");
    if (data.Unlocked.HasFlag(item))
      return Task.FromResult(
        $"{index}. {item.GetColor().GetChatColor()}{name} {ChatColors.Green}({ChatColors.Grey}Unlocked{ChatColors.Green})");
    return Task.FromResult(
      $"{index}. {item.GetColor().GetChatColor()}{name} {ChatColors.DarkRed}({ChatColors.LightRed}{item.GetCost()}{ChatColors.DarkRed})");
  }
}