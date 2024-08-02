using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;

namespace Jailbreak.English.SpecialDay;

public class SpeedrunDayMessages() : SoloDayMessages("Speedrunners",
  $"Follow the {ChatColors.Blue}blue{ChatColors.Default} player!",
  "They will run to a spot on the map.",
  $"Each round, the {ChatColors.Red}slowest players{ChatColors.Default} to reach the target will be eliminated.") {
  public IView RoundEnded
    => new SimpleView {
      SpecialDayMessages.PREFIX, "Round over! The next one will start shortly."
    };

  public IView NoneEliminated
    => new SimpleView {
      SpecialDayMessages.PREFIX, "No one was eliminated this round!"
    };

  public IView NoneReachedGoal
    => new SimpleView {
      {
        SpecialDayMessages.PREFIX,
        "Not enough players reached the goal this round!"
      },
      SimpleView.NEWLINE, {
        SpecialDayMessages.PREFIX,
        "Going off of distance to target for those who didn't."
      }
    };

  public IView YouAreRunner(int seconds) {
    return new SimpleView {
      { SpecialDayMessages.PREFIX, "You are the speedrunner!" },
      SimpleView.NEWLINE, {
        SpecialDayMessages.PREFIX, "You have", seconds,
        "seconds to run to a spot to set the goal."
      }
    };
  }

  public IView BeginRound(int round, int eliminationCount, int seconds) {
    return new SimpleView {
      {
        SpecialDayMessages.PREFIX, "Round #", round, "begins! The slowest",
        eliminationCount, "player" + (eliminationCount == 1 ? "" : "s"),
        "to reach the goal will be eliminated!"
      },
      SimpleView.NEWLINE, {
        SpecialDayMessages.PREFIX, "You have", seconds,
        "seconds to reach the goal!"
      }
    };
  }

  public IView RuntimeLeft(int seconds) {
    return new SimpleView {
      SpecialDayMessages.PREFIX,
      "You have",
      seconds,
      "seconds left to run to a spot!"
    };
  }

  public IView RunnerAssigned(CCSPlayerController player) {
    return new SimpleView {
      SpecialDayMessages.PREFIX,
      player,
      "is the speedrunner! Follow them closely!"
    };
  }

  public IView PlayerTime(CCSPlayerController player, int position,
    float time) {
    var place = position switch {
      1 => ChatColors.Green + "FIRST",
      2 => ChatColors.LightYellow + "Second",
      3 => ChatColors.BlueGrey + "3rd",
      _ => ChatColors.Grey + position + "th"
    };
    if (time < 0)
      return new SimpleView {
        SpecialDayMessages.PREFIX,
        player,
        "finished in",
        -time,
        "seconds.",
        place,
        "place" + (position == 1 ? "!" : ".")
      };

    return new SimpleView {
      SpecialDayMessages.PREFIX,
      player,
      "was",
      time,
      "units away from the goal,",
      place,
      "place."
    };
  }

  public IView PlayerEliminated(CCSPlayerController player) {
    return new SimpleView {
      SpecialDayMessages.PREFIX, player, "was eliminated!"
    };
  }

  public IView PlayerWon(CCSPlayerController player) {
    return new SimpleView {
      SpecialDayMessages.PREFIX, player, "won the game!"
    };
  }

  public IView BestTime(CCSPlayerController player, float time) {
    return new SimpleView {
      SpecialDayMessages.PREFIX,
      player,
      "beat the best time with",
      time,
      $"seconds! {ChatColors.Green}FIRST PLACE{ChatColors.Default}!"
    };
  }
}