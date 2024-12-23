using System.Drawing;
using Gangs.BaseImpl.Extensions;

namespace WardenPaintColorPerk;

[Flags]
public enum WardenPaintColor {
  DEFAULT = 1 << 0,
  RED = 1 << 1,
  ORANGE = 1 << 2,
  YELLOW = 1 << 3,
  GREEN = 1 << 4,
  CYAN = 1 << 5,
  BLUE = 1 << 6,
  PURPLE = 1 << 7,
  RANDOM = 1 << 8,
  RAINBOW = 1 << 9
}

public static class WardenColorExtensions {
  private static readonly Random rng = new();

  public static Color? GetColor(this WardenPaintColor paintColor) {
    return paintColor switch {
      WardenPaintColor.RED     => Color.Red,
      WardenPaintColor.ORANGE  => Color.Orange,
      WardenPaintColor.YELLOW  => Color.Yellow,
      WardenPaintColor.GREEN   => Color.Green,
      WardenPaintColor.CYAN    => Color.Cyan,
      WardenPaintColor.BLUE    => Color.Blue,
      WardenPaintColor.PURPLE  => Color.Purple,
      WardenPaintColor.DEFAULT => null,
      WardenPaintColor.RANDOM  => null,
      _                        => Color.White
    };
  }

  public static int GetCost(this WardenPaintColor paintColor) {
    if (paintColor == WardenPaintColor.RAINBOW) return 10 * 7500;
    if (paintColor == WardenPaintColor.DEFAULT) return 0;
    return (int)Math.Round(paintColor.GetColor().GetColorMultiplier() * 7500);
  }

  public static Color? PickRandom(this WardenPaintColor paintColor) {
    var n = rng.Next(Enum.GetValues<WardenPaintColor>().Length);
    var available = Enum.GetValues<WardenPaintColor>()
     .Where(c => paintColor.HasFlag(c) && c.GetColor() != null)
     .ToList();

    // Gang bought the random perk, but no colors, sillies!
    if (available.Count == 0) return null;

    return available[n % available.Count].GetColor();
  }
}