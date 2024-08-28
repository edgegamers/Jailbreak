using CounterStrikeSharp.API.Modules.Cvars.Validators;

namespace Jailbreak.Validator;

public class NonZeroRangeValidator<T>(T min, T max)
  : IValidator<T> where T : IComparable<T> {
  public bool Validate(T value, out string? errorMessage) {
    if (value.Equals(default(T))) {
      errorMessage = $"Value must be non-zero between {min} and {max}";
      return false;
    }

    if (value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0) {
      errorMessage = null;
      return true;
    }

    errorMessage = $"Value must be between {min} and {max}";
    return false;
  }
}