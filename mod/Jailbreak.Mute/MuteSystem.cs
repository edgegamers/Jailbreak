using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.Mute;
using Jailbreak.Public.Mod.Warden;
using Microsoft.Extensions.DependencyInjection;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Jailbreak.Mute;

public class MuteSystem(IServiceProvider provider) : IPluginBehavior, IMuteService
{
    private BasePlugin parent;
    private DateTime lastPeace = DateTime.MinValue;
    private DateTime peaceEnd = DateTime.MinValue;
    private DateTime ctPeaceEnd = DateTime.MinValue;

    private IPeaceMessages messages;
    private IWardenService warden;
    private Queue<int> ctScheduledMutes = new();
    private Queue<int> tScheduledMutes = new();


    private Timer? prisonerTimer, guardTimer;

    public void Start(BasePlugin parent)
    {
        this.parent = parent;

        messages = provider.GetRequiredService<IPeaceMessages>();
        warden = provider.GetRequiredService<IWardenService>();

        parent.RegisterListener<Listeners.OnClientVoice>(OnPlayerSpeak);
    }

    public void Dispose()
    {
        parent.RemoveListener("OnClientVoice", OnPlayerSpeak);
    }

    private void TickTerroristMutes()
    {
        if (tScheduledMutes.Count == 0)
            return;

        var muteDuration = tScheduledMutes.Dequeue();

        prisonerTimer = parent.AddTimer(muteDuration, () =>
        {
            if (tScheduledMutes.Count != 0)
            {
                TickTerroristMutes();
                return;
            }
            foreach (var player in Utilities.GetPlayers().Where(player => player.IsReal() && player.Team == CsTeam.Terrorist))
            {
                UnMute(player);
            }

            prisonerTimer?.Kill();
            prisonerTimer = null;
        });
    }

    private void TickCounterTerroristMutes()
    {
        if (ctScheduledMutes.Count == 0)
            return;

        var muteDuration = ctScheduledMutes.Dequeue();

        guardTimer = parent.AddTimer(muteDuration, () =>
        {
            if (ctScheduledMutes.Count != 0)
            {
                TickCounterTerroristMutes();
                return;
            }

            foreach (var player in Utilities.GetPlayers().Where(player => player.IsReal() && player.Team == CsTeam.CounterTerrorist))
            {
                UnMute(player);
            }


            guardTimer?.Kill();
            guardTimer = null;
        });
    }

    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        foreach (var player in Utilities.GetPlayers())
        {
            player.VoiceFlags &= VoiceFlags.ListenAll | VoiceFlags.ListenTeam;
            foreach (var target in Utilities.GetPlayers())
            {
                if (target == player)
                {
                    continue;
                }
                player.SetListenOverride(target, ListenOverride.Hear);
            }
        }
        return HookResult.Continue;
    }

    public void PeaceMute(MuteReason reason)
    {
        var duration = GetPeaceDuration(reason);
        var ctDuration = Math.Min(10, duration);

        if (IsPeaceEnabled())
        {
            foreach (var player in Utilities.GetPlayers().Where(player => player.IsReal() && (player.VoiceFlags & VoiceFlags.Muted) != 0))
                UnMute(player);
        }
        foreach (var player in Utilities.GetPlayers().Where(player => player.IsReal()))
            if (!warden.IsWarden(player))
                Mute(player);

        switch (reason)
        {
            case MuteReason.ADMIN:
                messages.PEACE_ENACTED_BY_ADMIN(duration).ToAllChat();
                break;
            case MuteReason.WARDEN_TAKEN:
                messages.GENERAL_PEACE_ENACTED(duration).ToAllChat();
                break;
            case MuteReason.WARDEN_INVOKED:
                messages.WARDEN_ENACTED_PEACE(duration).ToAllChat();
                break;
            case MuteReason.INITIAL_WARDEN:
                messages.GENERAL_PEACE_ENACTED(duration).ToAllChat();
                break;
        }

        this.peaceEnd = DateTime.Now.AddSeconds(duration);
        this.ctPeaceEnd = DateTime.Now.AddSeconds(ctDuration);
        this.lastPeace = DateTime.Now;

        guardTimer?.Kill();
        prisonerTimer?.Kill();

        ctScheduledMutes.Enqueue(ctDuration);
        tScheduledMutes.Enqueue(duration);

        if (tScheduledMutes.Count == 1 || prisonerTimer == null) TickTerroristMutes();
        if (ctScheduledMutes.Count == 1 || guardTimer == null) TickCounterTerroristMutes();
    }

    public void UnPeaceMute()
    {
        foreach (var player in Utilities.GetPlayers().Where(player => player.IsReal() && player.Team == CsTeam.Terrorist))
        {
            UnMute(player);
        }
        foreach (var player in Utilities.GetPlayers().Where(player => player.IsReal() && player.Team == CsTeam.CounterTerrorist))
        {
            UnMute(player);
        }
        prisonerTimer?.Kill();
        prisonerTimer = null;
        guardTimer?.Kill();
        guardTimer = null;
    }


    private int GetPeaceDuration(MuteReason reason)
    {
        var prisoners = Utilities.GetPlayers()
            .Count(c => c.IsReal() && c is { Team: CsTeam.Terrorist, PawnIsAlive: true });
        // https://www.desmos.com/calculator/gwd9cqw4yq
        var baseTime = (int)Math.Floor((prisoners + 30) / 5.0) * 5;

        return reason switch
        {
            MuteReason.ADMIN => baseTime,
            MuteReason.WARDEN_TAKEN => baseTime / 5,
            MuteReason.INITIAL_WARDEN => baseTime,
            MuteReason.WARDEN_INVOKED => baseTime / 2,
            _ => baseTime
        };
    }

    private void Mute(CCSPlayerController player)
    {
        if (BypassMute(player))
            return;
        player.VoiceFlags |= VoiceFlags.Muted;
    }

    private void UnMute(CCSPlayerController player)
    {
        player.VoiceFlags &= ~VoiceFlags.Muted;
    }

    public bool IsPeaceEnabled()
    {
        return DateTime.Now < peaceEnd;
    }

    public DateTime GetLastPeace()
    {
        return lastPeace;
    }

    private void OnPlayerSpeak(int playerSlot)
    {
        var player = Utilities.GetPlayerFromSlot(playerSlot);
        if (!player.IsReal())
            return;

        if (warden.IsWarden(player))
        {
            // Always let the warden speak
            UnMute(player);
            return;
        }

        if (!player.PawnIsAlive && !BypassMute(player))
        {
            // Normal players can't speak when dead
            messages.DEAD_REMINDER.ToPlayerCenter(player);
            Mute(player);
            return;
        }

        if (IsMuted(player))
        {
            // Remind any muted players they're muted
            messages.MUTE_REMINDER.ToPlayerCenter(player);
            return;
        }

        if (BypassMute(player))
        {
            // Warn admins if they're not muted
            if (IsPeaceEnabled())
            {
                if (player.Team == CsTeam.CounterTerrorist && DateTime.Now >= ctPeaceEnd)
                    return;
                messages.PEACE_REMINDER.ToPlayerCenter(player);
            }

            if (!player.PawnIsAlive)
                messages.ADMIN_DEAD_REMINDER.ToPlayerCenter(player);
        }
    }

    private bool IsMuted(CCSPlayerController player)
    {
        if (!player.IsReal())
            return false;
        return (player.VoiceFlags & VoiceFlags.Muted) != 0;
    }

    private bool BypassMute(CCSPlayerController player)
    {
        return player.IsReal() && AdminManager.PlayerHasPermissions(player, "@css/chat");
    }
}
