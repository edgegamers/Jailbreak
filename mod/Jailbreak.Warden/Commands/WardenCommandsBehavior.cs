using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Warden;

namespace Jailbreak.Warden.Commands;

public class WardenCommandsBehavior(IWardenNotifications notifications,
  IWardenSelectionService queue, IWardenService warden,
  IGenericCommandNotifications generics, WardenConfig config)
  : IPluginBehavior {
  private readonly Dictionary<CCSPlayerController, DateTime> lastPassCommand =
    new();

  [GameEventHandler]
  public HookResult OnRoundStart(EventRoundStart ev, GameEventInfo info) {
    lastPassCommand.Clear();
    return HookResult.Continue;
  }

  [ConsoleCommand("css_pass", "Pass warden onto another player")]
  [ConsoleCommand("css_uw", "Pass warden onto another player")]
  [CommandHelper(0, "", CommandUsage.CLIENT_ONLY)]
  public void Command_Pass(CCSPlayerController? player, CommandInfo command) {
    if (player == null) return;

    if (!warden.IsWarden(player)) return;

    //	Handle warden pass
    notifications.PassWarden(player).ToAllChat().ToAllCenter();

    // GetPlayers() returns valid players, no need to error check here.
    foreach (var clients in Utilities.GetPlayers())
      clients.ExecuteClientCommand(
        $"play sounds/{config.WardenPassedSoundName}");

    notifications.BecomeNextWarden.ToAllChat();

    if (!warden.TryRemoveWarden(true))
      Server.PrintToChatAll("[BUG] Couldn't remove warden :^(");

    lastPassCommand[player] = DateTime.Now;
  }

  [ConsoleCommand("css_fire", "Force the warden to pass")]
  [CommandHelper(0, "", CommandUsage.CLIENT_ONLY)]
  public void Command_Fire(CCSPlayerController? player, CommandInfo command) {
    if (player == null) return;

    if (!warden.HasWarden || warden.Warden == null) {
      notifications.CurrentWarden(null).ToPlayerChat(player);
      return;
    }

    if (!AdminManager.PlayerHasPermissions(player, "@css/ban")) {
      generics.NoPermissionMessage("@css/ban").ToPlayerChat(player);
      return;
    }

    foreach (var client in Utilities.GetPlayers()) {
      if (AdminManager.PlayerHasPermissions(client, "@css/chat"))
        notifications.FireWarden(warden.Warden, player).ToPlayerChat(client);
      else
        notifications.FireWarden(warden.Warden).ToPlayerChat(client);

      client.ExecuteClientCommand(
        $"play sounds/{config.WardenPassedSoundName}");
    }

    notifications.BecomeNextWarden.ToAllChat();

    lastPassCommand[warden.Warden] = DateTime.Now;

    if (!warden.TryRemoveWarden(true))
      Server.PrintToChatAll("[BUG] Couldn't remove warden :^(");
  }

  [ConsoleCommand("css_warden",
    "Become a warden, Join the warden queue, or see information about the current warden.")]
  [ConsoleCommand("css_w",
    "Become a warden, Join the warden queue, or see information about the current warden.")]
  [CommandHelper(0, "", CommandUsage.CLIENT_ONLY)]
  public void Command_Warden(CCSPlayerController? player, CommandInfo command) {
    if (player == null) return;

    // Why add them to a cooldown list if they can't even be warden :) 
    if (player.Team != CsTeam.CounterTerrorist || !player.PawnIsAlive) return;

    // If they're already in the cooldown dictionary, check if their cooldown has expired.
    if (lastPassCommand.TryGetValue(player, out var last)) {
      var cooldown = last.AddSeconds(15);
      if (DateTime.Now < cooldown) {
        generics.CommandOnCooldown(cooldown).ToPlayerChat(player);
        return;
      }
    }

    //  Queue is open ?
    if (queue.Active) {
      if (!queue.InQueue(player)) {
        if (queue.TryEnter(player))
          notifications.JoinRaffle.ToPlayerChat(player);
        return;
      }

      if (queue.InQueue(player))
        if (queue.TryExit(player))
          notifications.LeaveRaffle.ToPlayerChat(player);

      return;
    }

    //	Is a CT and there is no warden i.e. the queue is not open/active.
    if (!warden.HasWarden)
      if (warden.TrySetWarden(player))
        return;

    notifications.CurrentWarden(warden.Warden).ToPlayerChat(player);
  }

  /// <summary>
  ///   If the player who just died was the warden, clear the claim cooldown dictionary, so other CT's can claim!
  /// </summary>
  [GameEventHandler]
  public HookResult OnWardenDeath(EventPlayerDeath @event, GameEventInfo info) {
    var player = @event.Userid;
    if (player == null) return HookResult.Continue;

    if (player != warden.Warden) return HookResult.Continue;

    lastPassCommand.Clear();
    return HookResult.Continue;
  }
}