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

    public void Dispose()
    {
        parent.RemoveListener("OnClientVoice", OnPlayerSpeak);
    }

    public void PeaceMute(MuteReason reason)
    {
        var duration = GetPeaceDuration(reason);
        var ctDuration = Math.Min(10, duration);
        foreach (var player in Utilities.GetPlayers().Where(player => player.IsReal()))
            mute(player);

        switch (reason)
        {
            case MuteReason.ADMIN:
                messages.PEACE_ENACTED_BY_ADMIN(duration).ToAllChat();
                break;
            case MuteReason.WARDEN_TAKEN or MuteReason.INITIAL_WARDEN:
                messages.WARDEN_ENACTED_PEACE(duration);
                break;
            case MuteReason.WARDEN_INVOKED:
                messages.GENERAL_PEACE_ENACTED(duration);
                break;
        }

        this.peaceEnd = DateTime.Now.AddSeconds(duration);
        this.ctPeaceEnd = DateTime.Now.AddSeconds(ctDuration);
        this.lastPeace = DateTime.Now;

        guardTimer?.Kill();
        prisonerTimer?.Kill();

        guardTimer = parent.AddTimer(ctDuration, () =>
        {
            foreach (var player in Utilities.GetPlayers()
                         .Where(player =>
                             player.IsReal() && player is { Team: CsTeam.CounterTerrorist, PawnIsAlive: true }))
                unmute(player);

            messages.UNMUTED_GUARDS.ToAllChat();
        });

        prisonerTimer = parent.AddTimer(duration, () =>
        {
            foreach (var player in Utilities.GetPlayers()
                         .Where(player =>
                             player.IsReal() && player is { Team: CsTeam.Terrorist, PawnIsAlive: true }))
                unmute(player);

            messages.UNMUTED_PRISONERS.ToAllChat();
        });
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
        if (!player.IsReal() || warden.IsWarden(player))
            return;

        if (bypassMute(player))
        {
            if (IsPeaceEnabled())
                messages.PEACE_REMINDER.ToPlayerCenter(player);
            return;
        }

        if (!player.PawnIsAlive)
        {
            messages.DEAD_REMINDER.ToPlayerCenter(player);
            mute(player);
            return;
        }

        if (isMuted(player))
            messages.MUTE_REMINDER.ToPlayerCenter(player);
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