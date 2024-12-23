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
  ZZZ = 1 << 18,
  DEFAULT = 1 << 19
}

public static class BombIconExtensions {
  public static int GetCost(this BombIcon icon) {
    return icon switch {
      BombIcon.ANYASMUG    => 1750,
      BombIcon.C4_RED      => 2000,
      BombIcon.CACHINGA    => 25000,
      BombIcon.CLOWN_EMOJI => 15000,
      BombIcon.CSGO        => 3500,
      BombIcon.DOGE        => 12500,
      BombIcon.DOLLAR      => 100000,
      BombIcon.EGO         => 7500,
      BombIcon.GOAT        => 20000,
      BombIcon.IM          => 10000,
      BombIcon.IMBAD       => 12500,
      BombIcon.KZ          => 10000,
      BombIcon.OMEGALUL    => 25000,
      BombIcon.PEPEGA      => 20000,
      BombIcon.POOP        => 5000,
      BombIcon.STEAM       => 2500,
      BombIcon.THINKING    => 7500,
      BombIcon.UWUNUKE     => 5000,
      BombIcon.ZZZ         => 10000,
      _                    => 0
    };
  }

  public static string GetEquipment(this BombIcon icon) {
    return icon.ToString().ToLower();
  }
}