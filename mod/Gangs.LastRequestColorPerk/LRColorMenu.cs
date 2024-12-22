using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Gangs.BaseImpl.Extensions;
using GangsAPI.Data;
using GangsAPI.Menu;
using GangsAPI.Services.Commands;
using Jailbreak.Public.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Gangs.LastRequestColorPerk;

public class LRColorMenu(IServiceProvider provider, LRColor data)
  : AbstractPagedMenu<LRColor>(provider, NativeSenders.Chat) {
  private readonly ICommandManager commands =
    provider.GetRequiredService<ICommandManager>();

  // Method to sort smoke colors
  private int CompareSmokeColors(LRColor a, LRColor b) {
    // If icon is unlocked, it should be next
    // If both are unlocked, sort by cost (highest first)
    if (data.HasFlag(a)) {
      if (data.HasFlag(b)) return a.GetCost().CompareTo(b.GetCost());
      return -1;
    }

    // If both are locked, sort by cost (lowest first)
    return data.HasFlag(b) ? 1 : a.GetCost().CompareTo(b.GetCost());
  }


  override protected Task<List<LRColor>> GetItems(PlayerWrapper player) {
    var list = Enum.GetValues<LRColor>().ToList();
    list.Sort(CompareSmokeColors);
    list.Insert(0, 0);
    return Task.FromResult(list);
  }

  override protected Task HandleItemSelection(PlayerWrapper player,
    List<LRColor> items, int selectedIndex) {
    commands.ProcessCommand(player, CommandCallingContext.Chat, "css_sdcolor",
      items[selectedIndex].ToString());
    Close(player);
    return Task.CompletedTask;
  }

  override protected Task<string> FormatItem(PlayerWrapper player, int index,
    LRColor item) {
    var name = item.ToString().ToTitleCase();
    if (item == 0)
      return Task.FromResult(
        $" {ChatColors.DarkBlue}Gang Perks: {ChatColors.LightBlue}Special Day Colors");
    if (data.HasFlag(item))
      return Task.FromResult(
        $"{index}. {item.GetColor().GetChatColor()}{name} {ChatColors.Green}({ChatColors.Grey}Unlocked{ChatColors.Green})");
    return Task.FromResult(
      $"{index}. {item.GetColor().GetChatColor()}{name} {ChatColors.DarkRed}({ChatColors.LightRed}{item.GetCost()}{ChatColors.DarkRed})");
  }
}