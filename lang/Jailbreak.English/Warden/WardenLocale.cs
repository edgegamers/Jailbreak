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
    new HiddenFormatObject(
      $" {ChatColors.Green}[{ChatColors.Olive}WARDEN{ChatColors.Green}]") {
      //	Hide in panorama and center text
      Plain = false, Panorama = false, Chat = true
    };

  public IView PickingShortly
    => new SimpleView {
      { PREFIX, $"{ChatColors.Grey}Picking a warden shortly..." },
      SimpleView.NEWLINE, {
        PREFIX,
        $"{ChatColors.Grey}To enter the warden queue, type {ChatColors.Blue}!warden{ChatColors.Grey} in chat."
      }
    };

  public IView NoWardens
    => new SimpleView {
      PREFIX,
      $"No wardens in queue! The next player to run {ChatColors.Blue}!warden{ChatColors.White} will become a warden."
    };

  public IView WardenLeft
    => new SimpleView { PREFIX, "The warden has left the game." };

  public IView WardenDied
    => new SimpleView {
      PREFIX,
      $"{ChatColors.Red}The warden has {ChatColors.DarkRed}died{ChatColors.Red}! CTs must pursue {ChatColors.Blue}!warden{ChatColors.Red}."
    };

  public IView BecomeNextWarden
    => new SimpleView {
      PREFIX,
      $"{ChatColors.Grey}Type {ChatColors.Blue}!warden{ChatColors.Grey} to become the next warden"
    };

  public IView JoinRaffle
    => new SimpleView {
      PREFIX,
      $"{ChatColors.Grey}You've {ChatColors.Green}joined {ChatColors.Grey}the warden raffle."
    };

  public IView LeaveRaffle
    => new SimpleView {
      PREFIX,
      $"{ChatColors.Grey}You've {ChatColors.Red}left {ChatColors.Grey}the warden raffle."
    };

  public IView NotWarden
    => new SimpleView {
      PREFIX, $"{ChatColors.LightRed}You are not the warden."
    };

  public IView FireCommandFailed
    => new SimpleView {
      PREFIX, "The fire command has failed to work for some unknown reason..."
    };

  public IView PassWarden(CCSPlayerController player) {
    return new SimpleView { PREFIX, player, "resigned from being warden." };
  }

  public IView FireWarden(CCSPlayerController player) {
    return new SimpleView { PREFIX, player, "was fired from being warden." };
  }

  public IView
    FireWarden(CCSPlayerController player, CCSPlayerController admin) {
    return new SimpleView {
      PREFIX,
      admin,
      "fired",
      player,
      "from being warden."
    };
  }

  public IView NewWarden(CCSPlayerController player) {
    return new SimpleView { PREFIX, player, "is now the warden!" };
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
}