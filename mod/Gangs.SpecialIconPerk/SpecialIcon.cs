namespace Gangs.SpecialIconPerk;

[Flags]
public enum SpecialIcon {
  DEFAULT = 1 << 0,
  LOWER_DEFAULT = 1 << 1,
  CIRCLE = 1 << 2,
  SQUARE = 1 << 3,
  TRIANGLE = 1 << 4,
  ASTERISK = 1 << 5,
  DIAMOND = 1 << 6,
  HEART = 1 << 7,
  CLUB = 1 << 8,
  SPADE = 1 << 9,
  RANDOM = 1 << 10
}

public static class SpecialIconExtensions {
  private static readonly Random rng = new();

  public static string GetIcon(this SpecialIcon icon) {
    return icon switch {
      SpecialIcon.DEFAULT       => "S",
      SpecialIcon.LOWER_DEFAULT => "s",
      SpecialIcon.CIRCLE        => "⬤",
      SpecialIcon.SQUARE        => "■",
      SpecialIcon.TRIANGLE      => "▲",
      SpecialIcon.ASTERISK      => "✸",
      SpecialIcon.DIAMOND       => "♦",
      SpecialIcon.HEART         => "♥",
      SpecialIcon.CLUB          => "♣",
      SpecialIcon.SPADE         => "♠",
      SpecialIcon.RANDOM        => "?",
      _                         => "S"
    };
  }

  public static int GetCost(this SpecialIcon icon) {
    return icon switch {
      SpecialIcon.DEFAULT       => 0,
      SpecialIcon.LOWER_DEFAULT => 1250,
      SpecialIcon.CIRCLE        => 1000,
      SpecialIcon.SQUARE        => 1200,
      SpecialIcon.TRIANGLE      => 1300,
      SpecialIcon.ASTERISK      => 4000,
      SpecialIcon.DIAMOND       => 3000,
      SpecialIcon.HEART         => 3000,
      SpecialIcon.CLUB          => 3000,
      SpecialIcon.SPADE         => 2500,
      SpecialIcon.RANDOM        => 10000,
      _                         => 3000
    };
  }

  public static string PickRandom(this SpecialIcon icon) {
    var n = rng.Next(Enum.GetValues<SpecialIcon>().Length);
    var available = Enum.GetValues<SpecialIcon>()
     .Where(c => icon.HasFlag(c) && c != SpecialIcon.RANDOM)
     .ToList();

    // Gang bought the random perk, but no colors, sillies!
    return available.Count == 0 ?
      SpecialIcon.DEFAULT.GetIcon() :
      available[n % available.Count].GetIcon();
  }
}