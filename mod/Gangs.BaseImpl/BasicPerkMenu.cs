using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using GangsAPI;
using GangsAPI.Data;
using GangsAPI.Menu;
using GangsAPI.Perks;
using GangsAPI.Services;
using GangsAPI.Services.Commands;
using GangsAPI.Services.Menu;
using GangsAPI.Services.Player;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Gangs.BaseImpl;

public class BasicPerkMenu(IServiceProvider provider, IPerk perk)
  : AbstractMenu<string>(provider.GetRequiredService<IMenuManager>(),
    NativeSenders.Chat) {
  private readonly ICommandManager commands =
    provider.GetRequiredService<ICommandManager>();

  private readonly IEcoManager economy =
    provider.GetRequiredService<IEcoManager>();

  private readonly IStringLocalizer localizer =
    provider.GetRequiredService<IStringLocalizer>();

  private readonly IPlayerManager players =
    provider.GetRequiredService<IPlayerManager>();

  public override async Task Open(PlayerWrapper player) {
    var items = await GetItems(player);
    await Show(player, items);
  }

  public override async Task AcceptInput(PlayerWrapper player, int input) {
    if (input == 0) {
      await Menus.CloseMenu(player);
      return;
    }

    await HandleItemSelection(player, await GetItems(player), input);
  }

  override protected async Task<List<string>> GetItems(PlayerWrapper player) {
    var gangPlayer = await players.GetPlayer(player.Steam);

    if (gangPlayer?.GangId == null || gangPlayer.GangRank == null) return [];

    var cost = await perk.GetCost(gangPlayer);
    var title =
      $" {ChatColors.DarkBlue}Gang Perk: {ChatColors.Blue}{perk.Name}";
    var items = new List<string>();
    if (perk.Description != null)
      title += $"\n {ChatColors.LightBlue}{perk.Description}";
    items.Add(title);
    if (cost != null) {
      var color = await economy.CanAfford(player, cost.Value) ?
        ChatColors.Green :
        ChatColors.Red;
      items.Add($"{color}Purchase ({cost})");
    }

    items.Add("Close");
    return items;
  }

  override protected async Task HandleItemSelection(PlayerWrapper player,
    List<string> items, int selectedIndex) {
    if (items[selectedIndex].Contains("Close")) {
      await Menus.CloseMenu(player);
      return;
    }

    var gangPlayer = await players.GetPlayer(player.Steam);
    if (gangPlayer?.GangId == null || gangPlayer.GangRank == null) return;
    if (await perk.GetCost(gangPlayer) == null) {
      await Printer.Invoke(player,
        localizer.Get(MSG.PERK_UNPURCHASABLE_WITH_ITEM, perk.Name));
      return;
    }

    await commands.ProcessCommand(player, CommandCallingContext.Chat,
      "css_gang", "purchase", perk.StatId);
    await Menus.CloseMenu(player);
  }

  override protected Task<string> FormatItem(PlayerWrapper player, int index,
    string item) {
    return Task.FromResult(index == 0 ?
      item :
      $"{index}. {ChatColors.Grey}{item}");
  }
}