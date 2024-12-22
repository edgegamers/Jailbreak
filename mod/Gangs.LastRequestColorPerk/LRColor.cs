using System.Drawing;
using Gangs.BaseImpl.Extensions;

namespace Gangs.LastRequestColorPerk;

[Flags]
public enum LRColor {
  ORANGE = 1 << 0,
  YELLOW = 1 << 1,
  GREEN = 1 << 2,
  CYAN = 1 << 3,
  BLUE = 1 << 4,
  PURPLE = 1 << 5,
  DEFAULT = 1 << 6,
  RANDOM = 1 << 7,
  RAINBOW = 1 << 8
}

public static class LRColorExtensions {
  public static int GetCost(this LRColor color) {
    if (color == LRColor.RAINBOW) return 10;
    return (int)Math.Round(color.GetColor().GetColorMultiplier() * 8000);
  }

  public static Color? GetColor(this LRColor color) {
    return color switch {
      LRColor.ORANGE  => Color.Orange,
      LRColor.YELLOW  => Color.Yellow,
      LRColor.GREEN   => Color.Green,
      LRColor.CYAN    => Color.Cyan,
      LRColor.BLUE    => Color.Blue,
      LRColor.PURPLE  => Color.Purple,
      LRColor.DEFAULT => null,
      LRColor.RANDOM  => null,
      _               => Color.White
    };
  }

  public static Color? PickRandomColor(this LRColor color) {
    var n = new Random().Next(Enum.GetValues<LRColor>().Length);
    var available = Enum.GetValues<LRColor>()
     .Where(c => color.HasFlag(c) && c.GetColor() != null)
     .ToList();

    // Gang bought the random perk, but no colors, sillies!
    if (available.Count == 0) return null;

    return available[n % available.Count].GetColor();
  }
}