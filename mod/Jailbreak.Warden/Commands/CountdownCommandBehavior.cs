using CounterStrikeSharp.API;
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
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Objects;

public class CountdownCommandBehavior(IWardenService warden, IMuteService mute,
  IWardenLocale wardenLocale, IGenericCmdLocale generics) : IPluginBehavior {
  
  public static readonly FakeConVar<int> CV_WARDEN_MAX_COUNTDOWN =
    new("css_jb_warden_countdown_max",
      "The maximum duration for a countdown", 15);
  
  public static readonly FakeConVar<int> CV_WARDEN_MIN_COUNTDOWN =
    new("css_jb_warden_countdown_min",
      "The minimum duration for a countdown", 3);
  
  public static readonly FakeConVar<int> CV_WARDEN_DEFAULT_COUNTDOWN =
    new("css_jb_warden_countdown_default",
      "The default duration for a countdown", 5);

  private DateTime lastCountdown = DateTime.MinValue;
  private int countdownDuration;
  
  [ConsoleCommand("css_countdown",
    "Invokes a countdown "
    + "that will display in chat and notify Ts when to go (for a game or to follow a command) "
    + "If no duration is provided it will default to 5 seconds. "
    + "Maximum duration of 15 seconds. Minimum duration of 3 seconds.")]

  public void Command_Countdown(CCSPlayerController? executor, CommandInfo command) {
    // Set duration of countdown
    countdownDuration = CV_WARDEN_DEFAULT_COUNTDOWN.Value;
    
    // Ensure countdown duration is within acceptable range
    if (command.ArgCount == 2) {
      if (!int.TryParse(command.GetArg(1), out countdownDuration)) {
        generics.InvalidParameter(command.GetArg(1), "number");
        return;
      }

      if (countdownDuration <= 0) {
        generics.InvalidParameter(command.GetArg(1), "number greater than 0");
        return;
      }
    }

    if (countdownDuration < CV_WARDEN_MIN_COUNTDOWN.Value) {
      generics.InvalidParameter(command.GetArg(1), 
        $"number greater than or equal to {CV_WARDEN_MIN_COUNTDOWN.Value}");
      return;
    }
    
    if (countdownDuration > CV_WARDEN_MAX_COUNTDOWN.Value) {
      generics.InvalidParameter(command.GetArg(1), 
        $"number less than or equal to {CV_WARDEN_MAX_COUNTDOWN.Value}");
      return;
    }
    //
    
    // Attempt to enact peace
    bool success = EnactPeace(executor);

    if (!success) return;
    
    // Inform players of countdown
    StartCountDown(countdownDuration);
    
    // Create callbacks each second to notify players of countdown time remaining / completion of countdown
    for (int i = countdownDuration; i > 0; --i) {
      int current = i; // lambda capture
      Server.RunOnTick(Server.TickCount + (64 * (countdownDuration - current)), 
        () => PrintCountdownToPlayers(current));
    }
    Server.RunOnTick(Server.TickCount + (64 * countdownDuration), () => PrintGoToPlayers());
  }

  // Is this okay?
  // Feels like bad encapsulation
  private static readonly FormatObject PREFIX =
    new HiddenFormatObject($" {ChatColors.Red}Countdown>") {
      Plain = false, Panorama = false, Chat = true
    };

  private void StartCountDown(int duration) {
    new SimpleView { PREFIX, $"A {duration} second countdown has begun!" }.ToAllChat();
  }
  
  private void PrintCountdownToPlayers(int seconds) {
    new SimpleView { PREFIX, "Countdown: " + seconds }.ToAllChat();
  }

  private void PrintGoToPlayers() {
    new SimpleView { PREFIX, "GO! GO! GO!" }.ToAllChat();

    var players = Utilities.GetPlayers();
    foreach (var player in players) {
      player.ExecuteClientCommand("play \\sounds\\vo\\agents\\balkan\\radio_letsgo01.vsnd_c");
    }
  }
  //
  
  // Attempt to enact a period of peace for players to focus on the countdown
  private bool EnactPeace(CCSPlayerController? executor) {
    var fromWarden = executor != null && warden.IsWarden(executor);

    if (executor == null
      || AdminManager.PlayerHasPermissions(executor, "@css/cheats")) {
      // Server console or a high-admin is invoking the peace period, bypass cooldown
      mute.PeaceMute(MuteReason.ADMIN);
      lastCountdown = DateTime.Now;
      return true;
    }

    if (!fromWarden
      && AdminManager.PlayerHasPermissions(executor, "@css/chat")) {
      wardenLocale.NotWarden.ToChat(executor);
      mute.PeaceMute(MuteReason.ADMIN);
      lastCountdown = DateTime.Now;
      return true;
    }

    if (DateTime.Now - lastCountdown < TimeSpan.FromSeconds(60)) {
      generics.CommandOnCooldown(lastCountdown.AddSeconds(60))
       .ToChat(executor);
      return false;
    }

    mute.PeaceMute(fromWarden ? MuteReason.WARDEN_INVOKED : MuteReason.ADMIN);
    lastCountdown = DateTime.Now;
    return true;
  }
}