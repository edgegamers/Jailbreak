namespace Jailbreak.Public.Mod.SpecialDay;

using System;
using System.Collections;
using System.Collections.Generic;

public class ShuffleBag<T> : IEnumerable<T> {
  private List<T> items;
  private List<T> bag;
  private Random random;

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

  public void Add(T item) {
    items.Add(item);
    bag.Add(item);
  }

  public T GetNext() {
    if (bag.Count == 0) { RefillBag(); }

    int index = random.Next(bag.Count);
    T   item  = bag[index];
    bag.RemoveAt(index);

    return item;
  }

  private void RefillBag() { bag.AddRange(items); }

  public int Count {
    get { return bag.Count; }
  }

  public IEnumerator<T> GetEnumerator() {
    while (true) { yield return GetNext(); }
  }

  IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
}