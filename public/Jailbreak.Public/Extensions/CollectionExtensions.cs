namespace Jailbreak.Public.Extensions;

public static class CollectionExtensions {
  private static readonly Random RANDOM = new();

  public static void Shuffle<T>(this IList<T> list) {
    var n = list.Count;
    while (n > 1) {
      n--;
      var k = RANDOM.Next(n + 1);
      (list[k], list[n]) = (list[n], list[k]);
    }
  }

  public static void Shuffle<T>(this IEnumerable<T> enumerable) {
    var list = enumerable.ToList();
    list.Shuffle();
  }
}