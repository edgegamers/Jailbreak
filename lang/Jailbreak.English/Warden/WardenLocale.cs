using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Objects;
using Jailbreak.Formatting.Views.Warden;

namespace Jailbreak.English.Warden;

public class WardenLocale : IWardenLocale,
  ILanguage<Formatting.Languages.English> {
  public static readonly FormatObject PREFIX =
    new HiddenFormatObject($" {ChatColors.DarkBlue}Guard>") {
      //	Hide in panorama and center text
      Plain = false, Panorama = false, Chat = true
    };

  public static readonly FormatObject COMMAND_STANDS =
    new HiddenFormatObject($"The previous command stands for 10 seconds.");

  public IView PickingShortly
    => new SimpleView {
      PREFIX,
      $"Picking a warden shortly, type {ChatColors.BlueGrey}!warden{ChatColors.Grey} to enter the queue."
    };

  public IView NoWardens
    => new SimpleView {
      PREFIX,
      $"No one in queue. Next guard to {ChatColors.BlueGrey}!warden{ChatColors.Grey} will become warden."
    };

  public IView NowFreeday
    => new SimpleView {
      PREFIX,
      $"It is now a freeday! CTs must pursue {ChatColors.BlueGrey}!warden{ChatColors.Grey}."
    };

  public IView WardenLeft
    => new SimpleView { PREFIX, "The warden left the game.", COMMAND_STANDS };

  public IView WardenDied
    => new SimpleView {
      {
        PREFIX,
        $"The warden {ChatColors.Red}died{ChatColors.Grey}. It is a freeday!"
      },
      SimpleView.NEWLINE, {
        PREFIX,
        $"CTs must pursue {ChatColors.BlueGrey}!warden{ChatColors.Grey}."
      }
    };

  public IView BecomeNextWarden
    => new SimpleView {
      PREFIX,
      $"Type {ChatColors.BlueGrey}!warden{ChatColors.Grey} to become the warden."
    };

  public IView JoinRaffle
    => new SimpleView {
      PREFIX,
      $"You {ChatColors.White}joined {ChatColors.Grey}the warden raffle."
    };

  public IView LeaveRaffle
    => new SimpleView {
      PREFIX, $"You {ChatColors.Red}left {ChatColors.Grey}the warden raffle."
    };

  public IView NotWarden
    => new SimpleView {
      PREFIX, $"{ChatColors.LightRed}You are not the warden."
    };

  public IView FireCommandFailed
    => new SimpleView {
      PREFIX, "The fire command failed for some unknown reason..."
    };
  
  public IView TogglingNotEnabled
    => new SimpleView {
      PREFIX, "Toggling Auto-Warden is not supported on this server."
    };

  public IView PassWarden(CCSPlayerController player) {
    return new SimpleView {
      PREFIX, player, "resigned from warden.", COMMAND_STANDS
    };
  }

  public IView FireWarden(CCSPlayerController player) {
    return new SimpleView {
      PREFIX, player, "was fired from warden.", COMMAND_STANDS
    };
  }

  public IView
    FireWarden(CCSPlayerController player, CCSPlayerController admin) {
    return new SimpleView {
      PREFIX,
      admin,
      "fired",
      player,
      "from warden.",
      COMMAND_STANDS
    };
  }

  public IView NewWarden(CCSPlayerController player) {
    return new SimpleView { PREFIX, player, "is now the warden." };
  }
  
  public IView CurrentWarden(CCSPlayerController? player) {
    return player is not null ?
      new SimpleView { PREFIX, "The warden is", player, "." } :
      new SimpleView { PREFIX, "There is no warden." };
  }

  public IView CannotWardenDuringWarmup
    => new SimpleView { PREFIX, "You cannot warden during warmup." };

  public IView FireCommandSuccess(CCSPlayerController player) {
    return new SimpleView {
      PREFIX, player, "was fired and is no longer the warden."
    };
  }

  public IView MarkerPlaced() {
    return new SimpleView {
      PREFIX, "Marker placed."
    };
  }

  public IView MarkerRemoved(string marker) {
    return new SimpleView {
      PREFIX, $"{marker}{ChatColors.Grey} marker removed."
    };
  }

  public IView AutoWardenToggled(bool enabled) {
    return new SimpleView {
      PREFIX,
      ChatColors.Grey + "You",
      enabled ? ChatColors.Green + "enabled" : ChatColors.Red + "disabled",
      ChatColors.Grey + "Auto-Warden."
    };
  }
}