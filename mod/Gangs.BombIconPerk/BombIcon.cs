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
      BombIcon.ANYASMUG    => 750,
      BombIcon.C4_RED      => 500,
      BombIcon.CACHINGA    => 1000,
      BombIcon.CLOWN_EMOJI => 2000,
      BombIcon.CSGO        => 750,
      BombIcon.DOGE        => 1500,
      BombIcon.DOLLAR      => 10000,
      BombIcon.EGO         => 1000,
      BombIcon.GOAT        => 5000,
      BombIcon.IM          => 500,
      BombIcon.IMBAD       => 1000,
      BombIcon.KZ          => 1000,
      BombIcon.OMEGALUL    => 3500,
      BombIcon.PEPEGA      => 2500,
      BombIcon.POOP        => 650,
      BombIcon.STEAM       => 750,
      BombIcon.THINKING    => 800,
      BombIcon.UWUNUKE     => 2500,
      BombIcon.ZZZ         => 2000,
      _                    => 0
    };
  }

  public static string GetEquipment(this BombIcon icon) {
    return icon.ToString().ToLower();
  }
}