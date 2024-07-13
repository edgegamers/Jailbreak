using System.Collections;
using Jailbreak.Formatting.Core;

namespace Jailbreak.Formatting.Objects;

/// <summary>
///   Merges several FormatObjects into one.
///   This class will throw an error if any of it's descendant formatobjects are itself,
///   to prevent looping.
/// </summary>
public class TreeFormatObject : FormatObject, IEnumerable<FormatObject> {
  private readonly List<FormatObject> children = new();

  private int locked;

  public IEnumerator<FormatObject> GetEnumerator() {
    return children.GetEnumerator();
  }

  IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

  /// <summary>
  ///   Lock this object, execute callback, then unlock.
  /// </summary>
  /// <exception cref="Exception"></exception>
  private T @lock<T>(Func<T> callback) {
    //	Achieve a re-entrant mutex for this thread.
    //	We can re-enter this lock as long as we are in the same thread, but others are blocked.
    //	this allows us to do the internal increment safely.
    lock (this) {
      //	set _locked to 1 if value is 0
      var old = Interlocked.CompareExchange(ref locked, 1, 0);

      if (old == 1)
        throw new Exception(
          "Possible loop detected in TreeFormatObject! Already locked during traversal");

      var result = callback();
      locked = 0;

      return result;
    }
  }

  public override string ToPlain() {
    return @lock(() => {
      var childPlain = children.Select(child => child.ToPlain());
      return string.Join(' ', childPlain);
    });
  }

  public override string ToChat() {
    return @lock(() => {
      var childChat = children.Select(child => child.ToChat());
      return string.Join(' ', childChat);
    });
  }

  public override string ToPanorama() {
    return @lock(() => {
      var childPanorama = children.Select(child => child.ToPanorama());
      return string.Join(' ', childPanorama);
    });
  }

  public void Add(FormatObject formatObject) { children.Add(formatObject); }
}