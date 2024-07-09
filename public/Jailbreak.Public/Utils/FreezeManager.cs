using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Extensions;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.Public.Utils;

public class FreezeManager(BasePlugin plugin) {
  private static FreezeManager Manager;
  private readonly Dictionary<CCSPlayerController, Timer> _frozenPlayers = [];
  private readonly BasePlugin _plugin = plugin;

  public static void CreateInstance(BasePlugin plugin) {
    Manager = new FreezeManager(plugin);
  }

  public static void FreezePlayer(CCSPlayerController player, int delay) {
    if (!player.IsReal()) return;

    if (Manager._frozenPlayers.ContainsKey(player)) return;

    player.Freeze();

    Manager._frozenPlayers.Add(player, Manager._plugin.AddTimer(delay, () => {
      player.UnFreeze();
      Manager._frozenPlayers.Remove(player);
    }));
  }

  public static void UnfreezePlayer(CCSPlayerController player) {
    if (!player.IsReal()) return;

    if (!Manager._frozenPlayers.ContainsKey(player)) return;

    Manager._frozenPlayers[player].Kill();
    Manager._frozenPlayers.Remove(player);
    player.UnFreeze();
  }
}