using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Warden;

namespace Jailbreak.Warden.Commands;

public class WardenCommandsBehavior(
    IWardenSelectionService queue,
    IWardenService warden,
    IWardenNotifications notifications,
    IGenericCommandNotifications generics,
    WardenConfig config)
    : IPluginBehavior
{
    private readonly Dictionary<ulong, DateTime> lastPassCommand = new();

    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart ev, GameEventInfo info)
    {
        lastPassCommand.Clear();
        return HookResult.Continue;
    }

    [ConsoleCommand("css_pass", "Pass warden onto another player")]
    [ConsoleCommand("css_uw", "Pass warden onto another player")]
    [CommandHelper(0, "", CommandUsage.CLIENT_ONLY)]
    public void Command_Pass(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
            return;

        if (!warden.IsWarden(player)) return;


        lastPassCommand[player.SteamID] = DateTime.Now;

        //	Handle warden pass
        notifications.PASS_WARDEN(player)
            .ToAllChat()
            .ToAllCenter();

        foreach (var clients in Utilities.GetPlayers())
        {
            if (!clients.IsReal()) continue;
            clients.ExecuteClientCommand(
                $"play sounds/{config.WardenPassedSoundName}");
        }

        notifications.BECOME_NEXT_WARDEN.ToAllChat();

        if (!warden.TryRemoveWarden())
            Server.PrintToChatAll("[BUG] Couldn't remove warden :^(");
    }

    [ConsoleCommand("css_warden",
        "Become a warden, Join the warden queue, or see information about the current warden.")]
    [ConsoleCommand("css_w", "Become a warden, Join the warden queue, or see information about the current warden.")]
    [CommandHelper(0, "", CommandUsage.CLIENT_ONLY)]
    public void Command_Warden(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
            return;

        var isCt = player.GetTeam() == CsTeam.CounterTerrorist;

        //	Is a CT and queue is open
        if (isCt && queue.Active)
        {
            if (!queue.InQueue(player))
            {
                if (queue.TryEnter(player))
                    notifications.JOIN_RAFFLE.ToPlayerChat(player);
                return;
            }

            if (queue.InQueue(player))
                if (queue.TryExit(player))
                    notifications.LEAVE_RAFFLE.ToPlayerChat(player);

            return;
        }

        if (lastPassCommand.TryGetValue(player.SteamID, out var last))
        {
            var cooldown = last.AddSeconds(30);
            if (DateTime.Now < cooldown)
            {
                generics.CommandOnCooldown(cooldown).ToPlayerChat(player);
                return;
            }
        }

        //	Is a CT and there is no warden
        if (isCt && !warden.HasWarden)
            warden.TrySetWarden(player);

        notifications.CURRENT_WARDEN(warden.Warden).ToPlayerChat(player);
    }
}