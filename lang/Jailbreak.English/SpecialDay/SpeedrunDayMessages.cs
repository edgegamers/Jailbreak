using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Base;

namespace Jailbreak.English.SpecialDay;

public class SpeedrunDayMessages() : SoloDayMessages("Speedrunners",
  "Follow the blue player!", "They will run to a spot on the map.",
  "Each round, the slowest players to reach the same spot will be eliminated") {
  public IView YouAreRunner(int seconds) {
    return new SimpleView {
      { SpecialDayMessages.PREFIX, "You are the speedrunner!" },
      SimpleView.NEWLINE, {
        SpecialDayMessages.PREFIX, "You have", seconds,
        "seconds to run to a spot to set the goal."
      }
    };
  }

  public IView BeginRound(int round, int eliminationCount) {
    return new SimpleView {
      SpecialDayMessages.PREFIX,
      "Round #",
      round,
      "begins! The slowest",
      eliminationCount,
      "player" + (eliminationCount == 1 ? "" : "s"),
      "to reach the goal will be eliminated!"
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

  public IView
    PlayerTime(CCSPlayerController player, int position, float time) {
    return new SimpleView {
      SpecialDayMessages.PREFIX,
      player,
      "finished in",
      time,
      "seconds,",
      position,
      "place!"
    };
  }
}