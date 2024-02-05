using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Logs;

namespace Jailbreak.Logs;

public class LogsListeners : IPluginBehavior
{
    private readonly ILogService _logs;

    public LogsListeners(ILogService logs)
    {
        this._logs = logs;
    }

    public void Start(BasePlugin parent)
    {
        parent.RegisterEventHandler<EventPlayerHurt>(OnPlayerHurt);
        parent.RegisterEventHandler<EventGrenadeThrown>(OnGrenadeThrown);
        parent.HookEntityOutput("func_button", "OnPressed", OnButtonPressed);
    }

    private HookResult OnButtonPressed(CEntityIOOutput output, string name, CEntityInstance activator,
        CEntityInstance caller, CVariant value, float delay)
    {
        if (!activator.IsValid)
            return HookResult.Continue;
        var index = (int)activator.Index;
        var pawn = Utilities.GetEntityFromIndex<CCSPlayerPawn>(index);
        if (!pawn.IsValid)
            return HookResult.Continue;
        if (!pawn.OriginalController.IsValid)
            return HookResult.Continue;
        var ent = Utilities.GetEntityFromIndex<CBaseEntity>((int)caller.Index);
        if (!ent.IsValid)
            return HookResult.Continue;
        _logs.AddLogMessage(
            $"{_logs.FormatPlayer(pawn.OriginalController.Value!)} pressed a button {ent.Entity?.Name ?? "Unlabeled"} -> {output.Connections?.TargetDesc ?? "None"}");
        return HookResult.Continue;
    }

    private HookResult OnGrenadeThrown(EventGrenadeThrown @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!player.IsReal())
            return HookResult.Continue;
        var grenade = @event.Weapon;

        _logs.AddLogMessage($"{_logs.FormatPlayer(player)} threw a {grenade}");

        return HookResult.Continue;
    }

    private HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!player.IsReal())
            return HookResult.Continue;
        var attacker = @event.Attacker;

        var isWorld = attacker == null || !attacker.IsReal();
        var health = @event.DmgHealth;

        if (isWorld)
        {
            if (health > 0)
                _logs.AddLogMessage($"The world hurt {_logs.FormatPlayer(player)} for {health} damage");
            else
                _logs.AddLogMessage($"The world killed {_logs.FormatPlayer(player)}");
        }
        else
        {
            if (health > 0)
                _logs.AddLogMessage(
                    $"{_logs.FormatPlayer(attacker!)} hurt {_logs.FormatPlayer(player)} for {health} damage");
            else
                _logs.AddLogMessage($"{_logs.FormatPlayer(attacker!)} killed {_logs.FormatPlayer(player)}");
        }

        return HookResult.Continue;
    }
}