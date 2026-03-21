using System.Drawing;

namespace Jailbreak.Public.Mod.Warden.Enums;

public enum MarkerColor {
  RED,
  GREEN,
  BLUE,
  MAGENTA,
  YELLOW,
  PURPLE,
  PINK,
  CYAN,
  WHITE,
}

public static class MarkerColorExtensions {
  public static string ToDisplayName(this MarkerColor color)
    => color switch {
      MarkerColor.RED     => "Red",
      MarkerColor.GREEN   => "Green",
      MarkerColor.BLUE    => "Blue",
      MarkerColor.MAGENTA => "Magenta",
      MarkerColor.YELLOW  => "Yellow",
      MarkerColor.PURPLE  => "Purple",
      MarkerColor.PINK    => "Pink",
      MarkerColor.CYAN    => "Cyan",
      MarkerColor.WHITE   => "White",
      _                   => throw new ArgumentOutOfRangeException(
        nameof(color), color, null)
    };

  public static Color ToColor(this MarkerColor color)
    => color switch {
      MarkerColor.RED => Color.Red,
      MarkerColor.GREEN => Color.Green,
      MarkerColor.BLUE => Color.Blue,
      MarkerColor.MAGENTA => Color.Magenta,
      MarkerColor.YELLOW => Color.Yellow,
      MarkerColor.PURPLE => Color.Purple,
      MarkerColor.PINK => Color.Pink,
      MarkerColor.CYAN => Color.Cyan,
      MarkerColor.WHITE => Color.White,
      _ => throw new ArgumentOutOfRangeException(nameof(color), color, null)
    };

  public static MarkerColor ToMarkerColor(this string colorKey)
    => colorKey switch {
      "Red"     => MarkerColor.RED,
      "Green"   => MarkerColor.GREEN,
      "Blue"    => MarkerColor.BLUE,
      "Magenta" => MarkerColor.MAGENTA,
      "Yellow"  => MarkerColor.YELLOW,
      "Purple"  => MarkerColor.PURPLE,
      "Pink"    => MarkerColor.PINK,
      "Cyan"    => MarkerColor.CYAN,
      _ => MarkerColor.WHITE
    };
  /// <summary>
  /// Returns all marker shapes.
  /// </summary>
  public static IReadOnlyList<MarkerColor> All()
    => Enum.GetValues<MarkerColor>().ToList();
}