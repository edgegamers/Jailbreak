using CounterStrikeSharp.API.Modules.Utils;
using GangsAPI.Data;
using GangsAPI.Exceptions;
using GangsAPI.Extensions;
using GangsAPI.Services.Gang;
using GangsAPI.Services.Menu;
using Microsoft.Extensions.DependencyInjection;

namespace Gangs.BombIconPerk;

public class BombIconMenu(IServiceProvider provider, int gangId) : IMenu {
  private readonly IGangStatManager gangStats =
    provider.GetRequiredService<IGangStatManager>();

  private readonly IGangManager gangs =
    provider.GetRequiredService<IGangManager>();

  public async Task Open(PlayerWrapper player) {
    player.PrintToChat(ChatColors.DarkBlue + "Gang Perk: "
      + ChatColors.LightBlue + "Bomb Icon");

    var (success, data) =
      await gangStats.GetForGang<BombPerkData>(gangId, BombPerk.STAT_ID);
    if (!success || data == null) data = new BombPerkData();

    var unlocked = data.Unlocked;
    var equipped = data.Equipped;

    var index = 0;
    foreach (var icon in Enum.GetValues<BombIcon>()) {
      index++;
      if (unlocked.HasFlag(icon)) {
        player.PrintToChat(
          $"{index}. {ChatColors.LightBlue}{icon.ToString().ToTitleCase()} {ChatColors.LightRed}{icon.GetCost()}");
        continue;
      }

      if (equipped == icon) {
        player.PrintToChat(
          $"{index}. {ChatColors.LightBlue}{icon.ToString().ToTitleCase()} {ChatColors.Green}(Equipped)");
        continue;
      }

      player.PrintToChat(
        $"{index}. {ChatColors.LightBlue}{icon.ToString().ToTitleCase()} {ChatColors.LightYellow}(Select)");
    }
  }

  public Task Close(PlayerWrapper player) { return Task.CompletedTask; }

  public Task AcceptInput(PlayerWrapper player, int input) {
    return Task.CompletedTask;
  }
}