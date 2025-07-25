using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Mod.SpecialDay.Enums;

public enum SDType {
  CUSTOM,
  BHOP,
  FFA,
  FOG,
  GUNGAME,
  GHOST,
  HE,
  HNS,
  INFECTION,
  NOSCOPE,
  OITC,
  PACMAN,
  RJ,
  SNAKE,
  SPEEDRUN,
  TAG,
  TELEPORT,
  WARDAY
}

public static class SDTypeExtensions {
  public static SDType? FromString(string type) {
    if (Enum.TryParse<SDType>(type, true, out var result)) return result;
    type = type.ToLower().Replace(" ", "");
    return type switch {
      "freeforall"                                      => SDType.FFA,
      "hide" or "hideseek" or "seek"                    => SDType.HNS,
      "ns"                                              => SDType.NOSCOPE,
      "war"                                             => SDType.WARDAY,
      "tron"                                            => SDType.SNAKE,
      "gun"                                             => SDType.GUNGAME,
      "zomb" or "zombie"                                => SDType.INFECTION,
      "rocket" or "rj" or "marketgardner"               => SDType.RJ,
      "speed" or "speeders" or "speedrunners" or "race" => SDType.SPEEDRUN,
      "tp"                                              => SDType.TELEPORT,
      "he" or "grenade" or "grenades"                   => SDType.HE,
      "ghost" or "ghosts"                               => SDType.GHOST,
      "fog"                                             => SDType.FOG,
      _                                                 => null
    };
  }

  public static string?
    CanCall(this SDType type, CCSPlayerController? executor) {
    return type switch {
      SDType.HNS or SDType.SPEEDRUN =>
        AdminManager.PlayerHasPermissions(executor, "@ego/dssilver") ?
          null :
          $"You must be a {ChatColors.Green}Silver Supporter {ChatColors.Default}({ChatColors.LightBlue}https://edgm.rs/donate{ChatColors.Default}){ChatColors.Grey} to start this day.",
      // SDType.SPEEDRUN =>
      //   AdminManager.PlayerHasPermissions(executor, "@ego/dsgold") ?
      //     null :
      //     $"You must be a {ChatColors.Gold}Gold Supporter {ChatColors.Default}({ChatColors.LightBlue}https://edgm.rs/donate{ChatColors.Default}){ChatColors.Grey} to start this day.",
      _ => null
    };
  }
}