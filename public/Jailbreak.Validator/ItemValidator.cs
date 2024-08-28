using CounterStrikeSharp.API.Modules.Cvars.Validators;

namespace Jailbreak.Validator;

public class ItemValidator(
  WeaponType type = WeaponType.WEAPON | WeaponType.UTILITY,
  bool allowEmpty = true, bool allowMultiple = false) : IValidator<string> {
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
      return type.GetItems().Contains(weapon);
    }

    errorMessage = null;
    return true;
  }
}