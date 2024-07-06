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

    private Timer? prisonerTimer, guardTimer;

    public void Start(BasePlugin parent)
    {
        this.parent = parent;

        this.messages = provider.GetRequiredService<IPeaceMessages>();
        this.warden = provider.GetRequiredService<IWardenService>();

        parent.RegisterListener<Listeners.OnClientVoice>(OnPlayerSpeak);
    }

    [GameEventHandler]
    public void OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        UnPeaceMute();
    }

    [GameEventHandler]
    public void OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        UnPeaceMute();
    }

    public void Dispose()
    {
        parent.RemoveListener(OnPlayerSpeak);
    }

    public void PeaceMute(MuteReason reason)
    {
        var duration = GetPeaceDuration(reason);
        var ctDuration = Math.Min(10, duration);
        foreach (var player in Utilities.GetPlayers().Where(player => player.IsReal()))
            if (!warden.IsWarden(player))
                mute(player);

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

        peaceEnd = DateTime.Now.AddSeconds(duration);
        ctPeaceEnd = DateTime.Now.AddSeconds(ctDuration);
        lastPeace = DateTime.Now;

        guardTimer?.Kill();
        prisonerTimer?.Kill();

        guardTimer = parent.AddTimer(ctDuration, unmuteGuards);
        prisonerTimer = parent.AddTimer(duration, unmutePrisoners);
    }

    private void unmuteGuards()
    {
        foreach (var player in Utilities.GetPlayers()
                     .Where(player =>
                         player.IsReal() && player is { Team: CsTeam.CounterTerrorist, PawnIsAlive: true }))
            unmute(player);

        messages.UNMUTED_GUARDS.ToAllChat();
        guardTimer = null;
    }

    private void unmutePrisoners()
    {
        foreach (var player in Utilities.GetPlayers()
                     .Where(player =>
                         player.IsReal() && player is { Team: CsTeam.Terrorist, PawnIsAlive: true }))
            unmute(player);

        messages.UNMUTED_PRISONERS.ToAllChat();
        prisonerTimer = null;
    }

    public void UnPeaceMute()
    {
        if (guardTimer != null)
            unmuteGuards();

        if (prisonerTimer != null)
            unmutePrisoners();
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
            MuteReason.INITIAL_WARDEN => 2 * baseTime / 3,
            MuteReason.WARDEN_INVOKED => baseTime / 2,
            _ => baseTime
        };
    }

    private void mute(CCSPlayerController player)
    {
        if (bypassMute(player))
            return;
        player.VoiceFlags |= VoiceFlags.Muted;
    }

    private void unmute(CCSPlayerController player)
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
        if (player == null || !player.IsReal())
            return;

        if (warden.IsWarden(player))
        {
            // Always let the warden speak
            unmute(player);
            return;
        }

        if (!player.PawnIsAlive && !bypassMute(player))
        {
            // Normal players can't speak when dead
            messages.DEAD_REMINDER.ToPlayerCenter(player);
            mute(player);
            return;
        }

        if (isMuted(player))
        {
            // Remind any muted players they're muted
            messages.MUTE_REMINDER.ToPlayerCenter(player);
            return;
        }

        if (bypassMute(player))
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

    private bool isMuted(CCSPlayerController player)
    {
        if (!player.IsReal())
            return false;
        return (player.VoiceFlags & VoiceFlags.Muted) != 0;
    }

    private bool bypassMute(CCSPlayerController player)
    {
        return player.IsReal() && AdminManager.PlayerHasPermissions(player, "@css/chat");
    }
}