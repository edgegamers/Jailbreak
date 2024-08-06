using System.Collections;

namespace Jailbreak.SpecialDay.SpecialDays;

public class BasicLinkedList<T> : IEnumerable<T> {
  public BasicLinkedListNode<T>? First { get; private set; }
  public BasicLinkedListNode<T>? Last { get; private set; }

  public BasicLinkedList() { }

  public BasicLinkedList(IEnumerable<T> values) {
    foreach (var value in values) AddLast(value);
  }

  public void AddFirst(T value) {
    var node = new BasicLinkedListNode<T>(value);
    AddFirst(node);
  }

  public void AddFirst(BasicLinkedListNode<T> node) {
    if (First == null) { First = Last = node; } else {
      node.Next      = First;
      First.Previous = node;
      First          = node;
    }
  }

  public void AddLast(BasicLinkedListNode<T> node) {
    if (Last == null) { First = Last = node; } else {
      node.Previous = Last;
      Last.Next     = node;
      Last          = node;
    }
  }

  public void AddLast(T value) {
    var node = new BasicLinkedListNode<T>(value);
    AddLast(node);
  }

  public IEnumerator<T> GetEnumerator() {
    var current = First;
    while (current != null) {
      yield return current.Value;
      current = current.Next;
    }
  }

  public void AddAfter(BasicLinkedListNode<T> node, T value) {
    var newNode = new BasicLinkedListNode<T>(value);
    AddAfter(node, newNode);
  }

  public void AddAfter(BasicLinkedListNode<T> node,
    BasicLinkedListNode<T> value) {
    value.Previous = node;
    value.Next     = node.Next;
    if (node.Next != null) node.Next.Previous = value;
    node.Next = value;
    if (node == Last) Last = value;
  }

  public void AddBefore(BasicLinkedListNode<T> node,
    BasicLinkedListNode<T> value) {
    value.Previous = node.Previous;
    value.Next     = node;
    if (node.Previous != null) node.Previous.Next = value;
    node.Previous = value;
    if (node == First) First = value;
  }

  public void AddBefore(BasicLinkedListNode<T> node, T value) {
    var newNode = new BasicLinkedListNode<T>(value);
    AddBefore(node, newNode);
  }

  public void Clear() { First = Last = null; }

  IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
}

public class BasicLinkedListNode<T>(T value) {
  public T Value = value;
  public BasicLinkedListNode<T>? Next, Previous;
}