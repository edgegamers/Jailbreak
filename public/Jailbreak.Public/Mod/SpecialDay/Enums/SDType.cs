namespace Jailbreak.Public.Mod.SpecialDay.Enums;

public enum SDType {
  CUSTOM,
  FFA,
  GUNGAME,
  HNS,
  NOSCOPE,
  OITC,
  PACMAN,
  SNAKE,
  SPEEDRUN,
  TAG,
  WARDAY,
  TELEPORT,
  INFECTION
}

public static class SDTypeExtensions {
  public static SDType? FromString(string type) {
    if (Enum.TryParse<SDType>(type, true, out var result)) return result;
    type = type.ToLower().Replace(" ", "");
    return type switch {
      "freeforall"                                          => SDType.FFA,
      "hide" or "hideseek" or "seek"                        => SDType.HNS,
      "ns"                                                  => SDType.NOSCOPE,
      "war"                                                 => SDType.WARDAY,
      "tron"                                                => SDType.SNAKE,
      "gun"                                                 => SDType.GUNGAME,
      "zomb" or "zombie"                                    => SDType.INFECTION,
      "speed" or "speedrun" or "speeders" or "speedrunners" => SDType.SPEEDRUN,
      "tp"                                                  => SDType.TELEPORT,
      _                                                     => null
    };
  }
}