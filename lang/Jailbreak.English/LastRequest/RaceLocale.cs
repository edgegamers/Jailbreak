using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Views.LastRequest;

namespace Jailbreak.English.LastRequest;

// ReSharper disable ClassNeverInstantiated.Global
public class RaceLocale : LastRequestLocale, ILRRaceLocale {
  public IView EndRaceInstruction
    => new SimpleView {
      {
        PREFIX,
        $"Type {ChatColors.Blue}!endrace{ChatColors.White} to set the end point!"
      },
      SimpleView.NEWLINE, {
        PREFIX,
        $"Type {ChatColors.Blue}!endrace{ChatColors.White} to set the end point!"
      },
      SimpleView.NEWLINE, {
        PREFIX,
        $"Type {ChatColors.Blue}!endrace{ChatColors.White} to set the end point!"
      },
      SimpleView.NEWLINE
    };

  public IView RaceStartingMessage(CCSPlayerController prisoner) {
    return new SimpleView {
      {
        PREFIX, prisoner,
        " is starting a race. Pay attention to where they set the end point!"
      }
    };
  }

  public IView NotInRaceLR() {
    return new SimpleView {
      {
        PREFIX,
        $"You must be in a race {ChatColors.Blue + "!lr" + ChatColors.White} to use this command"
      }
    };
  }

  public IView NotInPendingState() {
    return new SimpleView {
      { PREFIX, "You must be in the pending state to use this command." }
    };
  }
}