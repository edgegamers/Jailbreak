using System.ComponentModel.Design;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.Warden;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Warden.Commands;

public class WardenCommandsBehavior(
    IWardenNotifications _notifications,
    IWardenSelectionService _queue,
    IWardenService _warden,
    IGenericCommandNotifications _generics,
    WardenConfig _config
    ) : IPluginBehavior
{

    private readonly Dictionary<CCSPlayerController, DateTime> _lastWardenCommand = new();

    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart ev, GameEventInfo info)
    {
        _lastWardenCommand.Clear();
        return HookResult.Continue;
    }

    [ConsoleCommand("css_pass", "Pass warden onto another player")]
    [ConsoleCommand("css_uw", "Pass warden onto another player")]
    [CommandHelper(0, "", CommandUsage.CLIENT_ONLY)]
    public void Command_Pass(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
            return;

        if (_warden.IsWarden(player))
        {
            //	Handle warden pass
            _notifications.PASS_WARDEN(player)
                .ToAllChat()
                .ToAllCenter();

            // GetPlayers() returns valid players, no need to error check here.
            foreach (var clients in Utilities.GetPlayers())
            {
                clients.ExecuteClientCommand(
                    $"play sounds/{_config.WardenPassedSoundName}");
            }

            _notifications.BECOME_NEXT_WARDEN.ToAllChat();

            if (!_warden.TryRemoveWarden(true))
                Server.PrintToChatAll("[BUG] Couldn't remove warden :^(");

            _lastWardenCommand.Clear();
            
        }
    }

    [ConsoleCommand("css_warden",
        "Become a warden, Join the warden queue, or see information about the current warden.")]
    [ConsoleCommand("css_w", "Become a warden, Join the warden queue, or see information about the current warden.")]
    [CommandHelper(0, "", CommandUsage.CLIENT_ONLY)]
    public void Command_Warden(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
            return;

        // Why add them to a cooldown list if they can't even be warden :) 
        if (player.Team != CsTeam.CounterTerrorist || !player.PawnIsAlive) { return; }

        // If they're already in the cooldown dictionary, check if their cooldown has expired.
        if (_lastWardenCommand.TryGetValue(player, out var last))
        {
            var cooldown = last.AddSeconds(8);
            if (DateTime.Now < cooldown)
            {
                _generics.CommandOnCooldown(cooldown).ToPlayerChat(player);
                return;
            }
        }
        
        // This makes an entry if the key doesn't exist (and here it doesn't).
        _lastWardenCommand[player] = DateTime.Now;

        //  Queue is open ?
        if (_queue.Active)
        {
            if (!_queue.InQueue(player))
            {
                if (_queue.TryEnter(player))
                    _notifications.JOIN_RAFFLE.ToPlayerChat(player);
                return;
            }

            if (_queue.InQueue(player))
                if (_queue.TryExit(player))
                    _notifications.LEAVE_RAFFLE.ToPlayerChat(player);

            return;
        }

        //	Is a CT and there is no warden i.e. the queue is not open/active.
        if (!_warden.HasWarden)
        {
            if (_warden.TrySetWarden(player))
                return;
        }

        _notifications.CURRENT_WARDEN(_warden.Warden).ToPlayerChat(player);
    }

    /// <summary>
    /// If the player who just died was the warden, clear the claim cooldown dictionary, so other CT's can claim!
    /// </summary>
    [GameEventHandler]
    public HookResult OnWardenDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;
        if (player == null) { return HookResult.Continue; }

        if (player != _warden.Warden) { return HookResult.Continue; }

        _lastWardenCommand.Clear();
        return HookResult.Continue;

    } 

}