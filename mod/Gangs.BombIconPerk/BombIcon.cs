namespace Gangs.BombIconPerk;

[Flags]
public enum BombIcon {
  ANYASMUG = 1 << 0,
  C4_RED = 1 << 1,
  CACHINGA = 1 << 2,
  CLOWN_EMOJI = 1 << 3,
  CSGO = 1 << 4,
  DOGE = 1 << 5,
  DOLLAR = 1 << 6,
  EGO = 1 << 7,
  GOAT = 1 << 8,
  IM = 1 << 9,
  IMBAD = 1 << 10,
  KZ = 1 << 11,
  OMEGALUL = 1 << 12,
  PEPEGA = 1 << 13,
  POOP = 1 << 14,
  STEAM = 1 << 15,
  THINKING = 1 << 16,
  UWUNUKE = 1 << 17,
  ZZZ = 1 << 18
}

public static class BombIconExtensions {
  public static int GetCost(this BombIcon icon) {
    return icon switch {
      BombIcon.ANYASMUG    => 1750,
      BombIcon.C4_RED      => 1500,
      BombIcon.CACHINGA    => 11000,
      BombIcon.CLOWN_EMOJI => 12000,
      BombIcon.CSGO        => 1750,
      BombIcon.DOGE        => 11500,
      BombIcon.DOLLAR      => 110000,
      BombIcon.EGO         => 11000,
      BombIcon.GOAT        => 15000,
      BombIcon.IM          => 1500,
      BombIcon.IMBAD       => 11000,
      BombIcon.KZ          => 11000,
      BombIcon.OMEGALUL    => 13500,
      BombIcon.PEPEGA      => 12500,
      BombIcon.POOP        => 1650,
      BombIcon.STEAM       => 1750,
      BombIcon.THINKING    => 1800,
      BombIcon.UWUNUKE     => 12500,
      BombIcon.ZZZ         => 12000,
      _                    => 0
    };
  }

  public static string GetEquipment(this BombIcon icon) {
    return icon.ToString().ToLower();
  }
}