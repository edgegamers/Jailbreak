using System.Drawing;
using CounterStrikeSharp.API.Modules.Utils;
using GangsAPI.Extensions;

namespace Gangs.SpecialDayColorPerk;

[Flags]
public enum SDColor {
  RED = 1 << 0,
  ORANGE = 1 << 1,
  YELLOW = 1 << 2,
  GREEN = 1 << 3,
  CYAN = 1 << 4,
  BLUE = 1 << 5,
  PURPLE = 1 << 6,
  DEFAULT = 1 << 7,
  RANDOM = 1 << 8
}

public static class SmokeColorExtensions {
  public static int GetCost(this SDColor color) {
    return color switch {
      SDColor.RED     => 10000,
      SDColor.ORANGE  => 4000,
      SDColor.YELLOW  => 3000,
      SDColor.GREEN   => 3500,
      SDColor.CYAN    => 8500,
      SDColor.BLUE    => 6000,
      SDColor.PURPLE  => 4000,
      SDColor.DEFAULT => 1,
      SDColor.RANDOM  => 35000,
      _               => 0
    };
  }

  public static Color? GetColor(this SDColor color) {
    return color switch {
      SDColor.RED     => Color.Red,
      SDColor.ORANGE  => Color.Orange,
      SDColor.YELLOW  => Color.Yellow,
      SDColor.GREEN   => Color.Green,
      SDColor.CYAN    => Color.Cyan,
      SDColor.BLUE    => Color.Blue,
      SDColor.PURPLE  => Color.Purple,
      SDColor.DEFAULT => null,
      SDColor.RANDOM  => null,
      _               => Color.White
    };
  }

  public static char GetChatColor(this SDColor color) {
    return color switch {
      SDColor.RED     => ChatColors.Red,
      SDColor.ORANGE  => ChatColors.Orange,
      SDColor.YELLOW  => ChatColors.Yellow,
      SDColor.GREEN   => ChatColors.Green,
      SDColor.CYAN    => ChatColors.LightBlue,
      SDColor.BLUE    => ChatColors.Blue,
      SDColor.PURPLE  => ChatColors.Purple,
      SDColor.DEFAULT => ChatColors.White,
      SDColor.RANDOM  => ChatColors.White,
      _               => ChatColors.White
    };
  }

  public static char GetChatColor(this Color color) {
    if (color == Color.Red) return ChatColors.Red;
    if (color == Color.Orange) return ChatColors.Orange;
    if (color == Color.Yellow) return ChatColors.Yellow;
    if (color == Color.Green) return ChatColors.Green;
    if (color == Color.Cyan) return ChatColors.LightBlue;
    if (color == Color.Blue) return ChatColors.Blue;
    if (color == Color.Purple) return ChatColors.Purple;
    return ChatColors.White;
  }

  public static Color? PickRandom(this SDColor color) {
    var n = new Random().Next(Enum.GetValues<SDColor>().Length);
    var available = Enum.GetValues<SDColor>()
     .Where(c => color.HasFlag(c) && c.GetColor() != null)
     .ToList();

    // Gang bought the random perk, but no colors, sillies!
    if (available.Count == 0) return null;

    return available[n % available.Count].GetColor();
  }
}