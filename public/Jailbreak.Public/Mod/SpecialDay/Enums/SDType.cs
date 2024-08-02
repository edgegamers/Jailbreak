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
  INFECTION
}

public static class SDTypeExtensions {
  public static SDType? FromString(string type) {
    if (Enum.TryParse<SDType>(type, true, out var result)) return result;
    type = type.ToLower().Replace(" ", "");
    switch (type) {
      case "freeforall":
        return SDType.FFA;
      case "hide":
      case "hideseek":
      case "seek":
        return SDType.HNS;
      case "ns":
        return SDType.NOSCOPE;
      case "war":
        return SDType.WARDAY;
      case "tron":
        return SDType.SNAKE;
      case "gun":
        return SDType.GUNGAME;
      case "zomb":
      case "zombie":
        return SDType.INFECTION;
      case "speed":
      case "speedrun":
      case "speeders":
      case "speedrunners":
        return SDType.SPEEDRUN;
    }

    return null;
  }
}