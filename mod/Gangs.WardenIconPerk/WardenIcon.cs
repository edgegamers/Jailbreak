namespace Gangs.WardenIconPerk;

[Flags]
public enum WardenIcon {
  DEFAULT = 1 << 0,
  LOWER_DEFAULT = 1 << 1,
  KATA_SMILE = 1 << 2,
  DOWN_ARROW = 1 << 3,
  STAR = 1 << 4,
  KING = 1 << 5,
  QUEEN = 1 << 6,
  ROOK = 1 << 7,
  BISHOP = 1 << 8,
  KNIGHT = 1 << 9,
  PAWN = 1 << 10,
  RANDOM = 1 << 11
}

public static class WardenIconExtensions {
  private static readonly Random rng = new();

  public static string GetIcon(this WardenIcon icon) {
    return icon switch {
      WardenIcon.KING          => "♔",
      WardenIcon.QUEEN         => "♕",
      WardenIcon.ROOK          => "♖",
      WardenIcon.BISHOP        => "♗",
      WardenIcon.KNIGHT        => "♘",
      WardenIcon.PAWN          => "♙",
      WardenIcon.DEFAULT       => "W",
      WardenIcon.LOWER_DEFAULT => "w",
      WardenIcon.KATA_SMILE    => "ツ",
      WardenIcon.DOWN_ARROW    => "↓",
      WardenIcon.STAR          => "★",
      WardenIcon.RANDOM        => "?",
      _                        => "W"
    };
  }

  public static int GetCost(this WardenIcon icon) {
    return icon switch {
      WardenIcon.DEFAULT       => 0,
      WardenIcon.LOWER_DEFAULT => 1500,
      WardenIcon.KATA_SMILE    => 4000,
      WardenIcon.DOWN_ARROW    => 3500,
      WardenIcon.STAR          => 6500,
      WardenIcon.KING          => 7000,
      WardenIcon.QUEEN         => 6500,
      WardenIcon.ROOK          => 5000,
      WardenIcon.BISHOP        => 2500,
      WardenIcon.KNIGHT        => 3000,
      WardenIcon.PAWN          => 4000,
      WardenIcon.RANDOM        => 10000,
      _                        => 5000
    };
  }

  public static string PickRandom(this WardenIcon icon) {
    var n = rng.Next(Enum.GetValues<WardenIcon>().Length);
    var available = Enum.GetValues<WardenIcon>()
     .Where(c => icon.HasFlag(c) && c != WardenIcon.RANDOM)
     .ToList();

    // Gang bought the random perk, but no colors, sillies!
    return available.Count == 0 ?
      WardenIcon.DEFAULT.GetIcon() :
      available[n % available.Count].GetIcon();
  }
}