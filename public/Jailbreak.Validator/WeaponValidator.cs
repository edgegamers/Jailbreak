using CounterStrikeSharp.API.Modules.Cvars.Validators;

namespace Jailbreak.Validator;

public class WeaponValidator(
  WeaponValidator.WeaponType type = WeaponValidator.WeaponType.WEAPON,
  bool allowEmpty = true) : IValidator<string> {
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
    if (string.IsNullOrWhiteSpace(value)) {
      errorMessage = allowEmpty ? null : "Value cannot be empty";
      return allowEmpty;
    }

    errorMessage = "Value must be a " + nameof(type).ToLower();
    return type switch {
      WeaponType.GRENADE  => Tag.GRENADES.Contains(value),
      WeaponType.UTILITY  => Tag.UTILITY.Contains(value),
      WeaponType.WEAPON   => Tag.GUNS.Contains(value),
      WeaponType.SNIPERS  => Tag.SNIPERS.Contains(value),
      WeaponType.RIFLES   => Tag.RIFLES.Contains(value),
      WeaponType.PISTOLS  => Tag.PISTOLS.Contains(value),
      WeaponType.SHOTGUNS => Tag.SHOTGUNS.Contains(value),
      WeaponType.SMGS     => Tag.SMGS.Contains(value),
      WeaponType.HEAVY    => Tag.HEAVY.Contains(value),
      _                   => throw new ArgumentOutOfRangeException("value")
    };
  }
}