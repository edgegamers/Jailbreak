using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Extensions;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.Public.Utils;

public class FreezeManager(BasePlugin plugin) {
  private static FreezeManager? _manager;
  private readonly Dictionary<CCSPlayerController, Timer> frozenPlayers = [];
  private readonly BasePlugin plugin = plugin;

  public static void CreateInstance(BasePlugin plugin) {
    _manager = new FreezeManager(plugin);
  }

  public static void FreezePlayer(CCSPlayerController player, int delay) {
    if (!player.IsReal()) return;

    if (_manager!.frozenPlayers.ContainsKey(player)) return;

    player.Freeze();

    _manager.frozenPlayers.Add(player, _manager.plugin.AddTimer(delay, () => {
      player.UnFreeze();
      _manager.frozenPlayers.Remove(player);
    }));
  }

  public static void UnfreezePlayer(CCSPlayerController player) {
    if (!player.IsReal()) return;

    if (!_manager!.frozenPlayers.TryGetValue(player, out var value)) return;
    value.Kill();
    _manager.frozenPlayers.Remove(player);
    player.UnFreeze();
  }
}