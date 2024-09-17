[Flags]
public enum WeaponType {
  GRENADE = 1 << 0,  // 1
  UTILITY = 1 << 1,  // 2
  WEAPON = 1 << 2,   // 4
  SNIPERS = 1 << 3,  // 8
  RIFLES = 1 << 4,   // 16
  PISTOLS = 1 << 5,  // 32
  SHOTGUNS = 1 << 6, // 64
  SMGS = 1 << 7,     // 128
  HEAVY = 1 << 8,    // 256
  GUNS = 1 << 9      // 512
}

public static class WeaponTypeExtensions {
  public static IReadOnlySet<string> GetItems(this WeaponType type) {
    var result = new HashSet<string>();

    switch (type) {
      case WeaponType.GUNS:
        return Tag.GUNS;
      case WeaponType.HEAVY:
        return Tag.HEAVY;
      case WeaponType.SMGS:
        return Tag.SMGS;
      case WeaponType.SHOTGUNS:
        return Tag.SHOTGUNS;
      case WeaponType.PISTOLS:
        return Tag.PISTOLS;
      case WeaponType.RIFLES:
        return Tag.RIFLES;
      case WeaponType.SNIPERS:
        return Tag.SNIPERS;
      case WeaponType.UTILITY:
        return Tag.UTILITY;
      case WeaponType.GRENADE:
        return Tag.GRENADES;
      case WeaponType.WEAPON:
        return Tag.WEAPONS;
      default:
        foreach (var t in Enum.GetValues<WeaponType>())
          if (type.HasFlag(t))
            result.UnionWith(t.GetItems());

        return result;
    }
  }
}