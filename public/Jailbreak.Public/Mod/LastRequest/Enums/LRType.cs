namespace Jailbreak.Public.Mod.LastRequest.Enums;

public enum LRType {
  GUN_TOSS,
  ROCK_PAPER_SCISSORS,
  KNIFE_FIGHT,
  NO_SCOPE,
  COINFLIP,
  SHOT_FOR_SHOT,
  MAG_FOR_MAG,
  RACE
}

public static class LRTypeExtensions {
  public static string ToFriendlyString(this LRType type) {
    return type switch {
      LRType.GUN_TOSS            => "Gun Toss",
      LRType.ROCK_PAPER_SCISSORS => "Rock Paper Scissors",
      LRType.KNIFE_FIGHT         => "Knife Fight",
      LRType.NO_SCOPE            => "No Scope",
      LRType.COINFLIP            => "Coinflip",
      LRType.SHOT_FOR_SHOT       => "Shot For Shot",
      LRType.MAG_FOR_MAG         => "Mag For Mag",
      LRType.RACE                => "Race",
      _                          => "Unknown"
    };
  }

  public static LRType FromIndex(int index) { return (LRType)index; }

  public static LRType? FromString(string type) {
    if (Enum.TryParse<LRType>(type, true, out var result)) return result;
    type = type.ToLower().Replace(" ", "");
    switch (type) {
      case "rps":
        return LRType.ROCK_PAPER_SCISSORS;
      case "s4s":
      case "sfs":
        return LRType.SHOT_FOR_SHOT;
      case "m4m":
      case "mfm":
        return LRType.MAG_FOR_MAG;
    }

    if (type.Contains("knife")) return LRType.KNIFE_FIGHT;
    if (type.Contains("scope")) return LRType.NO_SCOPE;
    if (type.Contains("gun")) return LRType.GUN_TOSS;
    if (type.Contains("coin") || type.Contains("fifty")) return LRType.COINFLIP;
    return null;
  }
}