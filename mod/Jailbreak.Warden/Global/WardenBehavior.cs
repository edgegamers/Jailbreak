using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Logs;
using Jailbreak.Public.Mod.Warden;
using Microsoft.Extensions.Logging;

namespace Jailbreak.Warden.Global;

public class WardenBehavior : IPluginBehavior, IWardenService
{
    private readonly ILogger<WardenBehavior> _logger;

    private readonly IWardenNotifications _notifications;
    private readonly ILogService _logs;

    public WardenBehavior(ILogger<WardenBehavior> logger, IWardenNotifications notifications, ILogService logs)
    {
        _logger = logger;
        _notifications = notifications;
        _logs = logs;
    }

    /// <summary>
    ///     Get the current warden, if there is one.
    /// </summary>
    public CCSPlayerController? Warden { get; private set; }

    /// <summary>
    ///     Whether or not a warden is currently assigned
    /// </summary>
    public bool HasWarden { get; private set; }

    public bool TrySetWarden(CCSPlayerController controller)
    {
        if (HasWarden)
            return false;

        //	Verify player is a CT
        if (controller.GetTeam() != CsTeam.CounterTerrorist)
            return false;
        if (!controller.PawnIsAlive)
            return false;

        HasWarden = true;
        Warden = controller;

        if (Warden.Pawn.Value != null)
        {
            Warden.Pawn.Value.RenderMode = RenderMode_t.kRenderTransColor;
            Warden.Pawn.Value.Render = Color.Blue;
            Utilities.SetStateChanged(Warden.Pawn.Value, "CBaseModelEntity", "m_clrRender");
        }

        _notifications.NewWarden(Warden)
            .ToAllChat()
            .ToAllCenter();

        _logs.AddLogMessage($"{Warden.PlayerName} is now the warden.");
        return true;
    }

    public bool TryRemoveWarden()
    {
        if (!HasWarden)
            return false;

        HasWarden = false;

        if (Warden != null && Warden.Pawn.Value != null)
        {
            Warden.Pawn.Value.RenderMode = RenderMode_t.kRenderTransColor;
            Warden.Pawn.Value.Render = Color.FromArgb(254, 255, 255, 255);
            Utilities.SetStateChanged(Warden.Pawn.Value, "CBaseModelEntity", "m_clrRender");
            _logs.AddLogMessage($"{Warden.PlayerName} is no longer the warden.");
        }

        Warden = null;

        return true;
    }

    [GameEventHandler]
    public HookResult OnDeath(EventPlayerDeath ev, GameEventInfo info)
    {
        if (!HasWarden)
            return HookResult.Continue;

        var player = ev.Userid;
        if (!player.IsReal())
            return HookResult.Continue;

        if (!((IWardenService)this).IsWarden(player))
            return HookResult.Continue;

        if (!TryRemoveWarden())
            _logger.LogWarning("[Warden] BUG: Problem removing current warden :^(");

        //	Warden died!
        _notifications.WardenDied
            .ToAllChat()
            .ToAllCenter();

        _notifications.BecomeNextWarden.ToAllChat();

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnRoundEnd(EventRoundEnd ev, GameEventInfo info)
    {
        TryRemoveWarden();

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerDisconnect(EventPlayerDisconnect ev, GameEventInfo info)
    {
        if (!HasWarden)
            return HookResult.Continue;

        var player = ev.Userid;

        if (!player.IsReal())
            return HookResult.Continue;

        if (!((IWardenService)this).IsWarden(ev.Userid))
            return HookResult.Continue;

        if (!TryRemoveWarden())
            _logger.LogWarning("[Warden] BUG: Problem removing current warden :^(");


        _notifications.WardenLeft
            .ToAllChat()
            .ToAllCenter();

        _notifications.BecomeNextWarden.ToAllChat();

        return HookResult.Continue;
    }
}