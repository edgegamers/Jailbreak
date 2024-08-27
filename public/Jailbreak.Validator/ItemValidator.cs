using CounterStrikeSharp.API.Modules.Cvars.Validators;

namespace Jailbreak.Validator;

public class ItemValidator(
  ItemValidator.WeaponType type = ItemValidator.WeaponType.WEAPON
    | ItemValidator.WeaponType.UTILITY, bool allowEmpty = true,
  bool allowMultiple = false) : IValidator<string> {
  [Flags]
  public enum WeaponType {
    GRENADE,
    UTILITY,
    WEAPON,
    SNIPERS,
    RIFLES,
    PISTOLS,
    SHOTGUNS,
    SMGS,
    HEAVY
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

      errorMessage = $"invalid {nameof(type).ToLower()}: {weapon}";
      return Enum.GetValues<WeaponType>()
       .Where(t => (t & type) == type)
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
      _ => throw new ArgumentOutOfRangeException(nameof(weapon))
    };
    return result;
  }
}