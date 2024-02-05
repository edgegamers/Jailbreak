using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Logs;

namespace Jailbreak.Logs;

public class LogsListeners : IPluginBehavior
{
    private ILogService logs;

    public LogsListeners(ILogService logs)
    {
        this.logs = logs;
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
        int index = (int)activator.Index;
        CCSPlayerPawn? pawn = Utilities.GetEntityFromIndex<CCSPlayerPawn>(index);
        if (!pawn.IsValid)
            return HookResult.Continue;
        if (!pawn.OriginalController.IsValid)
            return HookResult.Continue;
        CBaseEntity? ent = Utilities.GetEntityFromIndex<CBaseEntity>((int)caller.Index);
        if (!ent.IsValid)
            return HookResult.Continue;
        logs.AddLogMessage(
            $"{logs.FormatPlayer(pawn.OriginalController.Value!)} pressed a button {ent.Entity?.Name ?? "Unlabeled"} -> {output?.Connections?.TargetDesc ?? "None"}");
        return HookResult.Continue;
    }

    private HookResult OnGrenadeThrown(EventGrenadeThrown @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!player.IsReal())
            return HookResult.Continue;
        var grenade = @event.Weapon;

        logs.AddLogMessage($"{logs.FormatPlayer(player)} threw a {grenade}");

        return HookResult.Continue;
    }

    private HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!player.IsReal())
            return HookResult.Continue;
        var attacker = @event.Attacker;

        bool isWorld = attacker == null || !attacker.IsReal();
        int health = @event.Health;

        if (isWorld)
        {
            if (health > 0)
            {
                logs.AddLogMessage($"The world hurt {logs.FormatPlayer(player)} for {health} damage");
            }
            else
            {
                logs.AddLogMessage($"The world killed {logs.FormatPlayer(player)}");
            }
        }
        else
        {
            if (health > 0)
            {
                logs.AddLogMessage(
                    $"{logs.FormatPlayer(attacker!)} hurt {logs.FormatPlayer(player)} for {health} damage");
            }
            else
            {
                logs.AddLogMessage($"{logs.FormatPlayer(attacker!)} killed {logs.FormatPlayer(player)}");
            }
        }

        return HookResult.Continue;
    }
}