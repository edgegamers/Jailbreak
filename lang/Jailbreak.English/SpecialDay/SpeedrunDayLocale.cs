using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.SpecialDay;

public class SpeedrunDayLocale() : SoloDayLocale("Speedrunners",
    $"Follow the {ChatColors.Blue}blue{ChatColors.Default} player!",
    "They will run to a spot on the map.",
    $"Each round, the {ChatColors.Red}slowest players{ChatColors.Default} to reach the target will be eliminated."),
  ISpeedDayLocale {
  public IView RoundEnded
    => new SimpleView {
      PREFIX, "Round over! The next one will start shortly."
    };

  public IView NoneReachedGoal
    => new SimpleView {
      { PREFIX, "Not enough players reached the goal this round!" },
      SimpleView.NEWLINE,
      { PREFIX, "Going off of distance to target for those who didn't." }
    };

  public IView NoneEliminated
    => new SimpleView { PREFIX, "No one was eliminated this round!" };

  public IView YouAreRunner(int seconds) {
    return new SimpleView {
      { PREFIX, "You are the speedrunner!" },
      SimpleView.NEWLINE, {
        PREFIX, "You have", seconds, "seconds to run to a spot to set the goal."
      }
    };
  }

  public IView BeginRound(int round, int eliminationCount, int seconds) {
    if (eliminationCount == 1)
      return new SimpleView {
        {
          PREFIX,
          $"Round #{ChatColors.Yellow}{round}{ChatColors.Default} begins! The slowest",
          "player to reach the goal will be eliminated!"
        },
        SimpleView.NEWLINE,
        { PREFIX, "You have", seconds, "seconds to reach the goal!" }
      };

    return new SimpleView {
      {
        PREFIX,
        $"Round {ChatColors.Yellow}#{round}{ChatColors.Default} begins! The slowest",
        eliminationCount, "players to reach the goal will be eliminated!"
      },
      SimpleView.NEWLINE,
      { PREFIX, "You have", seconds, "seconds to reach the goal." }
    };
  }

  public IView RuntimeLeft(int seconds) {
    return new SimpleView {
      PREFIX, "You have", seconds, "seconds left to run to a spot!"
    };
  }

  public IView RunnerAssigned(CCSPlayerController player) {
    return new SimpleView {
      PREFIX, player, "is the speedrunner! Follow them closely!"
    };
  }

  public IView RunnerReassigned(CCSPlayerController player) {
    return new SimpleView {
      PREFIX,
      "The original speedrunner left, so",
      player,
      "is now the speedrunner!"
    };
  }

  public IView PlayerTime(CCSPlayerController player, int position,
    float time) {
    var place = position switch {
      1 => ChatColors.Green + "FIRST",
      2 => ChatColors.LightYellow + "Second",
      3 => ChatColors.BlueGrey + "3rd",
      _ => ChatColors.Grey + "" + position + "th"
    };
    if (time < 0)
      return new SimpleView {
        PREFIX,
        player,
        "finished in",
        -time,
        "seconds.",
        place,
        "place" + (position == 1 ? "!" : ".")
      };

    return new SimpleView {
      PREFIX,
      player,
      "was",
      time,
      "units away from the goal,",
      place,
      "place."
    };
  }

  public IView PlayerEliminated(CCSPlayerController player) {
    return new SimpleView { PREFIX, player, "was eliminated!" };
  }

  public IView PlayerWon(CCSPlayerController player) {
    return new SimpleView { PREFIX, player, "won the game!" };
  }

  public IView BestTime(CCSPlayerController player, float time) {
    return new SimpleView {
      PREFIX,
      player,
      "beat the best time with",
      time,
      $"seconds! {ChatColors.Green}FIRST PLACE{ChatColors.Default}!"
    };
  }

  public IView ImpossibleLocation(CsTeam team, CCSPlayerController player) {
    return new SimpleView {
      {
        PREFIX, "No one on", team,
        "reached the goal. Eliminating one player on each time."
      },
      SimpleView.NEWLINE,
      { PREFIX, "Randomly selected the path from ", player, "." }
    };
  }
}