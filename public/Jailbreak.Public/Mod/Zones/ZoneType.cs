using System.Drawing;

namespace Jailbreak.Public.Mod.Zones;

public enum ZoneType {
  /// <summary>
  ///   This zone captures the armory or places that CTs should otherwise not camp
  /// </summary>
  ARMORY,

  /// <summary>
  ///   This zone captures the cell areas
  /// </summary>
  CELL,

  /// <summary>
  ///   This zone is a single point, and represents the cell button
  /// </summary>
  CELL_BUTTON,

  /// <summary>
  ///   This zone captures a secret area that Ts may try to access
  /// </summary>
  SECRET,

  /// <summary>
  ///   This zone grants health
  /// </summary>
  HEALTH,

  /// <summary>
  ///   This zone supports many games, but is not a game-specific zone
  ///   e.g. playground, soccer field, trivia, etc.
  /// </summary>
  PLAYFIELD,

  /// <summary>
  ///   This zone is a single point, and represents a good spawn location
  /// </summary>
  SPAWN,

  /// <summary>
  ///   Similar to SPAWN, but this spawn location was automatically
  ///   generated
  /// </summary>
  SPAWN_AUTO,

  /// <summary>
  ///   This zone represents an area on the map that only Ts can access
  /// </summary>
  ZONE_LIMIT_T,

  /// <summary>
  ///   This zone represents an area on the map that only CTs can access
  /// </summary>
  ZONE_LIMIT_CT,

  /// <summary>
  ///   This zone represents an area on the map where it is possible to be
  ///   locked in.
  /// </summary>
  SOLITAIRE,

  /// <summary>
  ///   This zone is a single point, and represents
  ///   the center of the map (both vertically and horizontally)
  /// </summary>
  CENTER,

  /// <summary>
  ///   This zone represents an area that is a hazard to players, and which
  ///   should be avoided (especially for teleporting into).
  ///   May also represent a zone that is not guaranteed to be safe (eg: a platform
  ///   is moving around, and may not be safe to teleport to).
  /// </summary>
  HAZARD
}

public static class ZoneTypeExtensions {
  public static Color GetColor(this ZoneType type) {
    return type switch {
      ZoneType.ARMORY        => Color.Blue,
      ZoneType.CELL          => Color.Red,
      ZoneType.SECRET        => Color.Green,
      ZoneType.HEALTH        => Color.Yellow,
      ZoneType.PLAYFIELD     => Color.Orange,
      ZoneType.SPAWN         => Color.White,
      ZoneType.ZONE_LIMIT_T  => Color.OrangeRed,
      ZoneType.ZONE_LIMIT_CT => Color.LightBlue,
      ZoneType.SPAWN_AUTO    => Color.Gray,
      ZoneType.CELL_BUTTON   => Color.Aqua,
      ZoneType.HAZARD        => Color.DarkRed,
      ZoneType.SOLITAIRE     => Color.DarkOrange,
      _                      => Color.Magenta
    };
  }

  public static bool IsSinglePoint(this ZoneType type) {
    return type switch {
      ZoneType.CELL_BUTTON => true,
      ZoneType.SPAWN       => true,
      ZoneType.CENTER      => true,
      ZoneType.SPAWN_AUTO  => true,
      _                    => false
    };
  }

  public static bool DoNotTeleport(this ZoneType type) {
    return type switch {
      ZoneType.HAZARD        => true,
      ZoneType.SOLITAIRE     => true,
      ZoneType.ZONE_LIMIT_T  => true,
      ZoneType.ZONE_LIMIT_CT => true,
      _                      => false
    };
  }

  public static IEnumerable<ZoneType> DoNotTeleport() {
    return Enum.GetValues<ZoneType>().Where(DoNotTeleport);
  }
}