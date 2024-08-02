using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Core;
using Jailbreak.Public.Extensions;

namespace Jailbreak.Formatting.Objects;

public class TeamFormatObject(CsTeam team) : FormatObject {
  public override string ToChat() { return GetChatColor(team) + getName(); }

  public override string ToPanorama() { return getName(); }

  public override string ToPlain() { return getName(); }

  private string getName() {
    return team switch {
      CsTeam.CounterTerrorist => "Guards",
      CsTeam.Terrorist => "Prisoners",
      CsTeam.None => "No one",
      CsTeam.Spectator => "Spectators",
      _ => throw new ArgumentOutOfRangeException(nameof(team), team, null)
    };
  }

  public static char GetChatColor(CsTeam team) {
    return team switch {
      CsTeam.Terrorist        => ChatColors.Red,
      CsTeam.CounterTerrorist => ChatColors.Blue,
      CsTeam.None             => ChatColors.Default,
      CsTeam.Spectator        => ChatColors.Grey,
      _                       => throw new ArgumentOutOfRangeException()
    };
  }
}