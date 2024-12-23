using System.Drawing;
using Gangs.BaseImpl.Extensions;

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
  RANDOM = 1 << 8,
  RAINBOW = 1 << 9
}

public static class SDColorExtensions {
  public static int GetCost(this SDColor color) {
    if (color == SDColor.RAINBOW) return 10 * 2500;
    if (color == SDColor.DEFAULT) return 0;
    return (int)Math.Round(color.GetColor().GetColorMultiplier() * 2500);
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