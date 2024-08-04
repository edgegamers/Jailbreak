using CounterStrikeSharp.API.Modules.Cvars.Validators;

namespace Jailbreak.Validator;

public class WeaponValidator(
  WeaponValidator.WeaponType type = WeaponValidator.WeaponType.WEAPON,
  bool allowEmpty = true, bool allowMultiple = false) : IValidator<string> {
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
        errorMessage = allowEmpty ? null : "weapon cannot be empty";
        return allowEmpty;
      }

      errorMessage = $"invalid {nameof(type).ToLower()}: {weapon}";
      var result = type switch {
        WeaponType.GRENADE  => Tag.GRENADES.Contains(weapon),
        WeaponType.UTILITY  => Tag.UTILITY.Contains(weapon),
        WeaponType.WEAPON   => Tag.WEAPONS.Contains(weapon),
        WeaponType.SNIPERS  => Tag.SNIPERS.Contains(weapon),
        WeaponType.RIFLES   => Tag.RIFLES.Contains(weapon),
        WeaponType.PISTOLS  => Tag.PISTOLS.Contains(weapon),
        WeaponType.SHOTGUNS => Tag.SHOTGUNS.Contains(weapon),
        WeaponType.SMGS     => Tag.SMGS.Contains(weapon),
        WeaponType.HEAVY    => Tag.HEAVY.Contains(weapon),
        _                   => throw new ArgumentOutOfRangeException("weapon")
      };
      if (!result) return false;
    }

    errorMessage = null;
    return true;
  }
}