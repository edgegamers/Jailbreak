using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Commands.Targeting;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Debug.Subcommands;

public abstract class AbstractCommand(IServiceProvider services)
{
    private readonly IGenericCommandNotifications _lang = services.GetRequiredService<IGenericCommandNotifications>();
    protected readonly IServiceProvider Services = services;

    public abstract void OnCommand(CCSPlayerController? executor, WrappedInfo info);

    protected TargetResult? GetTarget(WrappedInfo command, int argIndex = 1,
        Func<CCSPlayerController, bool>? predicate = null)
    {
        return GetTarget(command.Info, argIndex + 1, predicate);
    }

    protected TargetResult? GetVulnerableTarget(WrappedInfo command, int argIndex = 1,
        Func<CCSPlayerController, bool>? predicate = null)
    {
        return GetVulnerableTarget(command.Info, argIndex + 1, predicate);
    }

    protected TargetResult? GetTarget(CommandInfo command, int argIndex = 1,
        Func<CCSPlayerController, bool>? predicate = null)
    {
        var matches = command.GetArgTargetResult(argIndex);

        matches.Players = matches.Players.Where(player =>
            player is { IsValid: true, Connected: PlayerConnectedState.PlayerConnected }).ToList();
        if (predicate != null)
            matches.Players = matches.Players.Where(predicate).ToList();

        if (!matches.Any())
        {
            if (command.CallingPlayer != null)
                _lang.PlayerNotFound(command.GetArg(argIndex)).ToPlayerChat(command.CallingPlayer);
            return null;
        }

        if (matches.Count() > 1 && command.GetArg(argIndex).StartsWith('@'))
            return matches;

        if (matches.Count() == 1 || !command.GetArg(argIndex).StartsWith('@'))
            return matches;

        if (command.CallingPlayer != null)
            _lang.PlayerFoundMultiple(command.GetArg(argIndex)).ToPlayerChat(command.CallingPlayer);
        return null;
    }

    protected TargetResult? GetVulnerableTarget(CommandInfo command, int argIndex = 1,
        Func<CCSPlayerController, bool>? predicate = null)
    {
        return GetTarget(command, argIndex,
            p => command.CallingPlayer == null ||
                 (command.CallingPlayer.CanTarget(p) && (predicate == null || predicate(p))));
    }

    protected TargetResult? GetSingleTarget(CommandInfo command, int argIndex = 1)
    {
        var matches = command.GetArgTargetResult(argIndex);

        if (!matches.Any())
        {
            if (command.CallingPlayer != null)
                _lang.PlayerNotFound(command.GetArg(argIndex)).ToPlayerChat(command.CallingPlayer);
            return null;
        }

        if (matches.Count() > 1)
        {
            if (command.CallingPlayer != null)
                _lang.PlayerFoundMultiple(command.GetArg(argIndex)).ToPlayerChat(command.CallingPlayer);
            return null;
        }

        return matches;
    }

    protected string GetTargetLabel(WrappedInfo info, int argIndex = 1)
    {
        return GetTargetLabel(info.Info, argIndex + 1);
    }

    protected string GetTargetLabel(CommandInfo info, int argIndex = 1)
    {
        switch (info.GetArg(argIndex))
        {
            case "@all":
                return "all players";
            case "@bots":
                return "all bots";
            case "@humans":
                return "all humans";
            case "@alive":
                return "alive players";
            case "@dead":
                return "dead players";
            case "@!me":
                return "all except self";
            case "@me":
                return info.CallingPlayer == null ? "Console" : info.CallingPlayer.PlayerName;
            case "@ct":
                return "all CTs";
            case "@t":
                return "all Ts";
            case "@spec":
                return "all spectators";
            default:
                var player = info.GetArgTargetResult(argIndex).FirstOrDefault();
                if (player != null)
                    return player.PlayerName;
                return "unknown";
        }
    }

    protected string GetTargetLabels(WrappedInfo info, int argIndex = 1)
    {
        return GetTargetLabels(info.Info, argIndex + 1);
    }

    protected string GetTargetLabels(CommandInfo info, int argIndex = 1)
    {
        var label = GetTargetLabel(info, argIndex);
        if (label.ToLower().EndsWith("s"))
            return label + "'";
        return label + "'s";
    }
}

public static class CommandExtensions

{
    internal static bool CanTarget(this CCSPlayerController controller, CCSPlayerController target)
    {
        if (!target.IsValid) return false;
        if (target.Connected != PlayerConnectedState.PlayerConnected) return false;
        if (target.IsBot || target.IsHLTV) return true;
        return AdminManager.CanPlayerTarget(controller, target);
    }
}