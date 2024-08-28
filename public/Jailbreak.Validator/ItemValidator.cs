using CounterStrikeSharp.API.Modules.Cvars.Validators;

namespace Jailbreak.Validator;

public class ItemValidator(
  ItemValidator.WeaponType type = ItemValidator.WeaponType.WEAPON
    | ItemValidator.WeaponType.UTILITY, bool allowEmpty = true,
  bool allowMultiple = false) : IValidator<string> {
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

  public bool Validate(string value, out string? errorMessage) {
    if (value.Contains(',') && !allowMultiple) {
      errorMessage = "Value cannot contain multiple values";
      return false;
    }

    if (string.IsNullOrWhiteSpace(value)) {
      errorMessage = allowEmpty ? null : "weapon cannot be empty";
      return allowEmpty;
    }

    foreach (var weapon in value.Split(',')) {
      if (string.IsNullOrWhiteSpace(weapon)) {
        if (!allowEmpty) {
          errorMessage = allowEmpty ? null : "weapon cannot be empty";
          return allowEmpty;
        }

        continue;
      }

      errorMessage = $"invalid {type.ToString()}: {weapon}";
      return Enum.GetValues<WeaponType>()
       .Where(t => type.HasFlag(t))
       .Any(t => validateType(t, weapon));
    }

    errorMessage = null;
    return true;
  }

  private bool validateType(WeaponType current, string weapon) {
    var result = current switch {
      WeaponType.GRENADE => Tag.GRENADES.Contains(weapon),
      WeaponType.UTILITY => Tag.UTILITY.Contains(weapon),
      WeaponType.WEAPON => Tag.WEAPONS.Contains(weapon),
      WeaponType.SNIPERS => Tag.SNIPERS.Contains(weapon),
      WeaponType.RIFLES => Tag.RIFLES.Contains(weapon),
      WeaponType.PISTOLS => Tag.PISTOLS.Contains(weapon),
      WeaponType.SHOTGUNS => Tag.SHOTGUNS.Contains(weapon),
      WeaponType.SMGS => Tag.SMGS.Contains(weapon),
      WeaponType.HEAVY => Tag.HEAVY.Contains(weapon),
      WeaponType.GUNS => Tag.GUNS.Contains(weapon),
      _ => throw new ArgumentOutOfRangeException(nameof(weapon))
    };
    return result;
  }
}