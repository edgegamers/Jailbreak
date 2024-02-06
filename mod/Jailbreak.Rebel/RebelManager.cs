using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Logs;
using Jailbreak.Public.Mod.Rebel;

namespace Jailbreak.Rebel;

public class RebelManager : IPluginBehavior, IRebelService
{
    private readonly ILogService _logs;
    private readonly IRebelNotifications _notifs;
    private readonly Dictionary<CCSPlayerController, long> _rebelTimes = new();

    public RebelManager(IRebelNotifications notifs, ILogService logs)
    {
        _notifs = notifs;
        _logs = logs;
    }

    public void Start(BasePlugin parent)
    {
        parent.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        parent.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        parent.RegisterEventHandler<EventRoundStart>(OnRoundStart);
        parent.RegisterListener<Listeners.OnTick>(OnTick);

        parent.AddTimer(1f, () =>
        {
            foreach (var player in GetActiveRebels())
            {
                if (!player.IsReal())
                    continue;

                if (GetRebelTimeLeft(player) <= 0)
                {
                    UnmarkRebel(player);
                    continue;
                }

                ApplyRebelColor(player);
                SendTimeLeft(player);
            }
        }, TimerFlags.REPEAT);
    }

    public ISet<CCSPlayerController> GetActiveRebels()
    {
        return _rebelTimes.Keys.ToHashSet();
    }

    public long GetRebelTimeLeft(CCSPlayerController player)
    {
        if (_rebelTimes.TryGetValue(player, out var time)) return time - DateTimeOffset.Now.ToUnixTimeSeconds();

        return 0;
    }

    public bool MarkRebel(CCSPlayerController player, long time = 120)
    {
        if (!_rebelTimes.ContainsKey(player)) _logs.AddLogMessage(player.PlayerName + " is now a rebel.");

        _rebelTimes[player] = DateTimeOffset.Now.ToUnixTimeSeconds() + time;
        ApplyRebelColor(player);
        return true;
    }

    public void UnmarkRebel(CCSPlayerController player)
    {
        _notifs.NoLongerRebel.ToPlayerChat(player);
        _logs.AddLogMessage(player.PlayerName + " is no longer a rebel.");

        _rebelTimes.Remove(player);
        ApplyRebelColor(player);
    }

    private void OnTick()
    {
        foreach (var player in GetActiveRebels())
        {
            if (!player.IsReal())
                continue;

            if (GetRebelTimeLeft(player) <= 0) continue;

            SendTimeLeft(player);
        }
    }

    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        _rebelTimes.Clear();
        foreach (var player in Utilities.GetPlayers())
        {
            if (!player.IsReal())
                continue;
            ApplyRebelColor(player);
        }

        return HookResult.Continue;
    }

    private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        if (_rebelTimes.ContainsKey(@event.Userid)) _rebelTimes.Remove(@event.Userid);

        return HookResult.Continue;
    }

    private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!player.IsReal())
            return HookResult.Continue;
        _rebelTimes.Remove(player);
        return HookResult.Continue;
    }

    // https://www.desmos.com/calculator/g2v6vvg7ax 
    private float GetRebelTimePercentage(CCSPlayerController player)
    {
        var x = GetRebelTimeLeft(player);
        if (x > 120)
            return 1;
        if (x <= 0)
            return 0;
        return (float)(100 - (120 - x) * Math.Sqrt(120 - x) / 13f) / 100;
    }

    private Color GetRebelColor(CCSPlayerController player)
    {
        var percent = GetRebelTimePercentage(player);
        var percentRgb = 255 - (int)Math.Round(percent * 255.0);
        var color = Color.FromArgb(254, 255, percentRgb, percentRgb);
        if (percent <= 0) color = Color.FromArgb(254, 255, 255, 255);

        return color;
    }

    private void ApplyRebelColor(CCSPlayerController player)
    {
        if (!player.IsReal() || player.Pawn.Value == null)
            return;
        var color = GetRebelColor(player);

        player.Pawn.Value.RenderMode = RenderMode_t.kRenderTransColor;
        player.Pawn.Value.Render = color;
        Utilities.SetStateChanged(player.Pawn.Value, "CBaseModelEntity", "m_clrRender");
    }

    private void SendTimeLeft(CCSPlayerController player)
    {
        // var timeLeft = GetRebelTimeLeft(player);
        // var formattedTime = TimeSpan.FromSeconds(timeLeft).ToString(@"mm\:ss");
        var color = GetRebelColor(player);
        var formattedColor = $"<font color=\"#{color.R:X2}{color.G:X2}{color.B:X2}\">";

        player.PrintToCenterHtml($"You are {formattedColor}<b>rebelling</b></font>");
    }
}