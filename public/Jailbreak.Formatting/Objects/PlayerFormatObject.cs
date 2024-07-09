using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Core;
using Jailbreak.Public.Extensions;

namespace Jailbreak.Formatting.Objects;

public class PlayerFormatObject(CCSPlayerController player) : FormatObject {
  private readonly string name = player.PlayerName;

  public override string ToChat() { return $"{ChatColors.Yellow}{name}"; }

  public override string ToPanorama() { return name.Sanitize(); }

  public override string ToPlain() { return name; }
}