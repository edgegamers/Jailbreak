using GangsAPI.Data;
using GangsAPI.Services.Menu;

namespace Gangs.BombIconPerk;

public class BombIconMenu : IMenu {
  public Task Open(PlayerWrapper player) {
    player.PrintToChat("Bomb Icon Menu");
    return Task.CompletedTask;
  }

  public Task Close(PlayerWrapper player) { return Task.CompletedTask; }

  public Task AcceptInput(PlayerWrapper player, int input) {
    return Task.CompletedTask;
  }
}