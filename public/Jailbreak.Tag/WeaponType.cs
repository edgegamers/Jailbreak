namespace Jailbreak.Tag;

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
        return global::Tag.GUNS;
      case WeaponType.HEAVY:
        return global::Tag.HEAVY;
      case WeaponType.SMGS:
        return global::Tag.SMGS;
      case WeaponType.SHOTGUNS:
        return global::Tag.SHOTGUNS;
      case WeaponType.PISTOLS:
        return global::Tag.PISTOLS;
      case WeaponType.RIFLES:
        return global::Tag.RIFLES;
      case WeaponType.SNIPERS:
        return global::Tag.SNIPERS;
      case WeaponType.UTILITY:
        return global::Tag.UTILITY;
      case WeaponType.GRENADE:
        return global::Tag.GRENADES;
      case WeaponType.WEAPON:
        return global::Tag.WEAPONS;
      default:
        foreach (var t in Enum.GetValues<WeaponType>())
          if (type.HasFlag(t))
            result.UnionWith(t.GetItems());

        return result;
    }
  }
}