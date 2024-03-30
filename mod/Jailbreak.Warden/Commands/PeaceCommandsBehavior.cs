using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.Mute;
using Jailbreak.Public.Mod.Warden;

namespace Jailbreak.Warden.Commands;

public class PeaceCommandsBehavior(
    IWardenService warden,
    IMuteService mute,
    IPeaceMessages messages,
    IWardenNotifications notifications,
    IGenericCommandNotifications generics)
    : IPluginBehavior
{
    [ConsoleCommand("css_peace", "Invokes a peace period where only the warden can talk")]
    public void Command_Peace(CCSPlayerController? executor, CommandInfo info)
    {
        if (mute.IsPeaceEnabled())
        {
            if (executor != null)
                messages.PEACE_REMINDER.ToPlayerChat(executor);
            return;
        }

        if (executor == null || AdminManager.PlayerHasPermissions(executor, "@css/cheats"))
        {
            mute.PeaceMute(MuteReason.ADMIN);
            return;
        }

        if (!warden.IsWarden(executor) && !AdminManager.PlayerHasPermissions(executor, "@css/chat"))
        {
            notifications.NOT_WARDEN.ToPlayerChat(executor);
            return;
        }

        bool admin = !warden.IsWarden(executor);

        if (DateTime.Now - mute.GetLastPeace() < TimeSpan.FromSeconds(60))
        {
            generics.CommandOnCooldown(DateTime.Now.AddSeconds(60)).ToPlayerChat(executor);
            return;
        }

        mute.PeaceMute(admin ? MuteReason.ADMIN : MuteReason.WARDEN_INVOKED);
    }
}