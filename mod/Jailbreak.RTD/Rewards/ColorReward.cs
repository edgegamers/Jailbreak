using System.Drawing;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.RTD;

namespace Jailbreak.RTD.Rewards;

public class ColorReward(Color color, bool prisonerOnly) : IRTDReward {
  public string ColorName
    => color.IsNamedColor ?
      color.Name :
      color.GetHue() switch {
        0   => "Red",
        60  => "Yellow",
        120 => "Green",
        180 => "Cyan",
        240 => "Blue",
        300 => "Magenta",
        _   => "#" + color.ToArgb().ToString("X8")
      };

  public char ChatColor
    => color.GetHue() switch {
      0   => ChatColors.Red,
      60  => ChatColors.Yellow,
      120 => ChatColors.Green,
      180 => ChatColors.LightBlue,
      240 => ChatColors.Blue,
      300 => ChatColors.Magenta,
      _   => ChatColors.White
    };

  public virtual string Name => "Spawn " + ColorName;

  public virtual string Description
    => $"You will spawn {ChatColor}{ColorName}{ChatColors.Grey} next round.";

  public bool CanGrantReward(CCSPlayerController player) {
    return player.Team == CsTeam.Terrorist || !prisonerOnly;
  }

  public bool GrantReward(CCSPlayerController player) {
    player.SetColor(color);
    return true;
  }
}