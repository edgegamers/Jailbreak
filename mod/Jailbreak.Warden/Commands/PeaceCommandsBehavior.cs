using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using Jailbreak.Formatting.Extensions;
using Jailbreak.Formatting.Views;
using Jailbreak.Formatting.Views.Warden;
using Jailbreak.Public.Behaviors;
using Jailbreak.Public.Mod.Mute;
using Jailbreak.Public.Mod.Warden;

namespace Jailbreak.Warden.Commands;

public class PeaceCommandsBehavior(IWardenService warden, IMuteService mute,
  IWardenPeaceLocale peaceLocale, IWardenLocale wardenLocale,
  IGenericCmdLocale generics) : IPluginBehavior {
  [ConsoleCommand("css_peace",
    "Invokes a peace period where only the warden can talk")]
  public void Command_Peace(CCSPlayerController? executor, CommandInfo info) {
    if (mute.IsPeaceEnabled()) {
      if (executor != null) peaceLocale.PeaceActive.ToChat(executor);
      return;
    }

    var fromWarden = executor != null && warden.IsWarden(executor);

    if (executor == null
      || AdminManager.PlayerHasPermissions(executor, "@css/cheats")) {
      // Server console or a high-admin is invoking the peace period, bypass cooldown
      mute.PeaceMute(fromWarden ? MuteReason.WARDEN_INVOKED : MuteReason.ADMIN);
      return;
    }

    if (!warden.IsWarden(executor)
      && !AdminManager.PlayerHasPermissions(executor, "@css/chat")) {
      wardenLocale.NotWarden.ToChat(executor);
      return;
    }

    if (DateTime.Now - mute.GetLastPeace() < TimeSpan.FromSeconds(60)) {
      generics.CommandOnCooldown(mute.GetLastPeace().AddSeconds(60))
       .ToChat(executor);
      return;
    }

    mute.PeaceMute(fromWarden ? MuteReason.WARDEN_INVOKED : MuteReason.ADMIN);
  }
}