using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using GangsAPI.Data;
using GangsAPI.Menu;
using GangsAPI.Services.Commands;
using Jailbreak.Public.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Gangs.BaseImpl;

public abstract class AbstractEnumMenu<T>(IServiceProvider provider, T data,
  T equipped, string command, string title, string desc)
  : AbstractPagedMenu<T>(provider, NativeSenders.Chat) where T : Enum {
  private readonly ICommandManager commands =
    provider.GetRequiredService<ICommandManager>();

  abstract protected int getCost(T item);
  abstract protected List<T> getValues();
  abstract protected string formatItem(T item);

  virtual protected int compare(T a, T b) {
    if (a.Equals(equipped)) return -1;
    if (b.Equals(equipped)) return 1;

    if (data.HasFlag(a)) {
      if (data.HasFlag(b)) return getCost(a).CompareTo(getCost(b));
      return -1;
    }

    return data.HasFlag(b) ? 1 : getCost(a).CompareTo(getCost(b));
  }

  override protected Task<List<T>> GetItems(PlayerWrapper player) {
    var list = getValues();
    list.Sort(compare);
    list.Insert(0, (T)(object)0);
    return Task.FromResult(list);
  }

  override protected Task HandleItemSelection(PlayerWrapper player,
    List<T> items, int selectedIndex) {
    commands.ProcessCommand(player, CommandCallingContext.Chat, command,
      items[selectedIndex].ToString());
    Menus.CloseMenu(player);
    return Task.CompletedTask;
  }

  override protected Task<string> FormatItem(PlayerWrapper player, int index,
    T item) {
    if (item.Equals((T)(object)0)) {
      return Task.FromResult(
        $" {ChatColors.DarkBlue}Gang Perks: {ChatColors.LightBlue}{title}\n {ChatColors.Grey}{desc}");
    }

    if (item.Equals(equipped))
      return Task.FromResult(
        $"{index} {formatItem(item)} {ChatColors.Green}({ChatColors.Lime}Equipped{ChatColors.Green})");

    if (data.HasFlag(item))
      return Task.FromResult(
        $"{index}. {formatItem(item)} {ChatColors.Green}({ChatColors.Grey}Unlocked{ChatColors.Green})");

    return Task.FromResult(
      $"{index}. {formatItem(item)} {ChatColors.DarkRed}({ChatColors.LightRed}{getCost(item)}{ChatColors.DarkRed})");
  }
}