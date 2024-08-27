using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Utils;

namespace Jailbreak.Validator;

public class TeamValidator(bool allowSpecs = true) : IValidator<string> {
  public bool Validate(string value, out string? errorMessage) {
    errorMessage = null;

    var team = TeamUtil.FromString(value);

    if (team == null) {
      errorMessage = $"Unknown team: \"{value}\"";
      return false;
    }

    if (team is CsTeam.None or CsTeam.Spectator && !allowSpecs) {
      errorMessage = "Team must be CT or T";
      return false;
    }

    return true;
  }
}