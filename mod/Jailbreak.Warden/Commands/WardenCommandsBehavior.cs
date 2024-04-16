using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Mute;
using Jailbreak.Public.Mod.Warden;

namespace Jailbreak.Warden.Commands;

public class WardenCommandsBehavior : IPluginBehavior
{
    private readonly IWardenNotifications _notifications;
    private readonly IWardenSelectionService _queue;
    private readonly IWardenService _warden;
    private readonly IGenericCommandNotifications _generics;
    private readonly IMuteService _mute;

    private readonly WardenConfig _config;
    private readonly Dictionary<CCSPlayerController, DateTime> _lastWardenCommand = new();

    public WardenCommandsBehavior(IWardenSelectionService queue, IWardenService warden,
        IWardenNotifications notifications, IGenericCommandNotifications generics, IMuteService mute, WardenConfig config)
    {
        _config = config;
        _queue = queue;
        _warden = warden;
        _generics = generics;
        _notifications = notifications;
        _mute = mute;
    }

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

            foreach (var clients in Utilities.GetPlayers())
            {
                if (!clients.IsReal()) continue;
                clients.ExecuteClientCommand(
                    $"play sounds/{_config.WardenPassedSoundName}");
            }

            _notifications.BECOME_NEXT_WARDEN.ToAllChat();

            if (!_warden.TryRemoveWarden())
                Server.PrintToChatAll("[BUG] Couldn't remove warden :^(");
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

        if (_lastWardenCommand.TryGetValue(player, out var last))
        {
            var cooldown = last.AddSeconds(15);
            if (DateTime.Now < cooldown)
            {
                _generics.CommandOnCooldown(cooldown).ToPlayerChat(player);
                return;
            }
        }
        
        _lastWardenCommand[player] = DateTime.Now;

        var isCt = player.GetTeam() == CsTeam.CounterTerrorist;

        //	Is a CT and queue is open
        if (isCt && _queue.Active)
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

        //	Is a CT and there is no warden
        if (isCt && !_warden.HasWarden)
            _warden.TrySetWarden(player);

        _notifications.CURRENT_WARDEN(_warden.Warden).ToPlayerChat(player);
    }

    [ConsoleCommand("css_fire", "Fires the currently active warden, if any.")]
    [CommandHelper(0, "", CommandUsage.CLIENT_AND_SERVER)]
    public void Command_FireWarden(CCSPlayerController? player, CommandInfo command)
    {
        CCSPlayerController? warden = _warden.Warden;

        if (player == null || warden == null) { return; }
        if (!AdminManager.PlayerHasPermissions(player, "@css/eg")) { return; }

        bool success = _warden.TryRemoveWarden();

        if (!success)
            _notifications.FIRE_COMMAND_FAILED.ToPlayerChat(player);
        else
        {
            _notifications.FIRE_COMMAND_SUCCESS(warden).ToAllChat();
            _mute.RemovePeaceMute();

            // TEMP SOLUTION. Eventually we want to use EmitSound in the world instead, waiting for CS#... 
            foreach (var p in Utilities.GetPlayers())
            {
                if (!p.IsReal()) continue;
                p.ExecuteClientCommand(
                    $"play sounds/{_config.WardenKilledSoundName}");
            }

        }

    }

}