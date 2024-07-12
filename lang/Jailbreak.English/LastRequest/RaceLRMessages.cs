using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.LastRequest;

// ReSharper disable ClassNeverInstantiated.Global
public class RaceLRMessages : IRaceLRMessages,
  ILanguage<Formatting.Languages.English> {
  public IView EndRaceInstruction
    => new SimpleView {
      {
        LastRequestMessages.PREFIX,
        $"Type ${ChatColors.Blue}!endrace${ChatColors.White} to set the end point!"
      },
      SimpleView.NEWLINE, {
        LastRequestMessages.PREFIX,
        $"Type ${ChatColors.Blue}!endrace${ChatColors.White} to set the end point!"
      },
      SimpleView.NEWLINE, {
        LastRequestMessages.PREFIX,
        $"Type ${ChatColors.Blue}!endrace${ChatColors.White} to set the end point!"
      },
      SimpleView.NEWLINE
    };

  public IView RaceStartingMessage(CCSPlayerController prisoner) {
    return new SimpleView {
      {
        LastRequestMessages.PREFIX, prisoner,
        " is starting a race. Pay attention to where they set the end point!"
      }
    };
  }

  public IView NotInRaceLR()
    => new SimpleView {
      {
        LastRequestMessages.PREFIX,
        $"You must be in a race {ChatColors.Blue + "!lr" + ChatColors.White} to use this command"
      }
    };

  public IView NotInPendingState()
    => new SimpleView {
      {
        LastRequestMessages.PREFIX,
        "You must be in the pending state to use this command."
      }
    };
}