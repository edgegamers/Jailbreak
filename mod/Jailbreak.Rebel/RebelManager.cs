using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Timers;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Logs;
using Jailbreak.Public.Mod.Rebel;

namespace Jailbreak.Teams;

public class RebelManager : IPluginBehavior, IRebelService
{
    private Dictionary<CCSPlayerController, long> rebelTimes = new();
    private IRebelNotifications notifs;
    private ILogService logs;

    public RebelManager(IRebelNotifications notifs, ILogService logs)
    {
        this.notifs = notifs;
        this.logs = logs;
    }

    public void Start(BasePlugin parent)
    {
        parent.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        parent.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        parent.RegisterEventHandler<EventRoundStart>(OnRoundStart);

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

    HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        rebelTimes.Clear();
        foreach (var player in Utilities.GetPlayers())
        {
            if (!player.IsReal())
                continue;
            ApplyRebelColor(player);
        }

        return HookResult.Continue;
    }

    HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        if (rebelTimes.ContainsKey(@event.Userid))
        {
            rebelTimes.Remove(@event.Userid);
        }

        return HookResult.Continue;
    }

    HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!player.IsReal())
            return HookResult.Continue;
        rebelTimes.Remove(player);
        return HookResult.Continue;
    }

    public ISet<CCSPlayerController> GetActiveRebels()
    {
        return rebelTimes.Keys.ToHashSet();
    }

    public long GetRebelTimeLeft(CCSPlayerController player)
    {
        if (rebelTimes.TryGetValue(player, out long time))
        {
            return time - DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        return 0;
    }

    public bool MarkRebel(CCSPlayerController player, long time)
    {
        if (!rebelTimes.ContainsKey(player))
        {
            logs.AddLogMessage(player.PlayerName + " is now a rebel.");
        }

        rebelTimes[player] = DateTimeOffset.Now.ToUnixTimeSeconds() + time;
        ApplyRebelColor(player);
        return true;
    }

    public void UnmarkRebel(CCSPlayerController player)
    {
        notifs.NO_LONGER_REBEL.ToPlayerChat(player);
        logs.AddLogMessage(player.PlayerName + " is no longer a rebel.");

        rebelTimes.Remove(player);
        ApplyRebelColor(player);
    }

    // https://www.desmos.com/calculator/g2v6vvg7ax 
    private float GetRebelTimePercentage(CCSPlayerController player)
    {
        long x = GetRebelTimeLeft(player);
        if (x > 120)
            return 1;
        if (x <= 0)
            return 0;
        return (float)(100 - (120 - x) * (Math.Sqrt(120 - x)) / 13f) / 100;
    }
    
    private Color GetRebelColor(CCSPlayerController player)
    {
        var percent = GetRebelTimePercentage(player);
        var percentRGB = 255 - (int)Math.Round(percent * 255.0);
        var color = Color.FromArgb(254, 255, percentRGB, percentRGB);
        if (percent <= 0)
        {
            color = Color.FromArgb(254, 255, 255, 255);
        }

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
        var timeLeft = GetRebelTimeLeft(player);
        var formattedTime = TimeSpan.FromSeconds(timeLeft).ToString(@"mm\:ss");
        var color = GetRebelColor(player);
        var formattedColor = $"<font color=\"#{color.R:X2}{color.G:X2}{color.B:X2}\">";
        
        player.PrintToCenterHtml($"You are a rebel for {formattedColor}{formattedTime}</font> more seconds.");
    }
}