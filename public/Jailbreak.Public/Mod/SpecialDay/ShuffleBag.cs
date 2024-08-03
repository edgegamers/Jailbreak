using System.Collections;

namespace Jailbreak.Public.Mod.SpecialDay;

public class ShuffleBag<T> : IEnumerable<T> {
  private readonly List<T> bag;
  private readonly List<T> items;
  private readonly Random random;

  public ShuffleBag() {
    items  = new List<T>();
    bag    = new List<T>();
    random = new Random();
  }

  public ShuffleBag(IEnumerable<T> items) {
    this.items = items.ToList();
    bag        = new List<T>();
    random     = new Random();
  }

  public int Count => bag.Count;

  public IEnumerator<T> GetEnumerator() {
    while (true) yield return GetNext();
    // ReSharper disable IteratorNeverReturns
  }
  // ReSharper restore IteratorNeverReturns

  IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

  public void Add(T item) {
    items.Add(item);
    bag.Add(item);
  }

  public T GetNext() {
    if (bag.Count == 0) RefillBag();

    var index = random.Next(bag.Count);
    var item  = bag[index];
    bag.RemoveAt(index);

    return item;
  }

  private void RefillBag() { bag.AddRange(items); }
}