using System.Collections;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Extensions;

namespace Jailbreak.Public.Mod.Trail;

public abstract class AbstractTrail<T>(float lifetime = 20, int maxPoints = 100)
  : IEnumerable<T> where T : ITrailSegment {
  // Ordered from newest to oldest (0 is the newest)
  protected readonly IList<T> Segments = new List<T>();

  public virtual float Lifetime {
    get => lifetime;
    set => lifetime = value;
  }

  public virtual int MaxPoints {
    get => maxPoints;
    set => maxPoints = value;
  }

  public IEnumerator<T> GetEnumerator() { return Segments.GetEnumerator(); }

  IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

  public virtual T? GetStartSegment() { return Segments.LastOrDefault(); }
  public virtual T? GetEndSegment() { return Segments.FirstOrDefault(); }

  virtual protected void Cleanup() {
    while (Segments.Count > MaxPoints) {
      var seg = Segments[^1];
      seg.Remove();
      Segments.RemoveAt(Segments.Count - 1);
    }
  }

  public virtual IList<T> GetTrail(float since, int max = 0) {
    var result = new List<T>();
    foreach (var segment in Segments) {
      if (segment.GetTimeAlive() > since) break;
      result.Add(segment);
      if (max > 0 && result.Count >= max) break;
    }

    return result;
  }

  public virtual IList<Vector> GetTrailPoints(float since, int max = 0) {
    var result = new List<Vector>();
    foreach (var segment in GetTrail(since, max)) {
      result.Add(segment.GetStart());
      result.Add(segment.GetEnd());
    }

    return result;
  }

  public virtual IList<Vector> GetTrailPoints(int max) {
    return GetTrailPoints(Lifetime, max);
  }

  public IList<Vector> GetTrailPoints() { return GetTrailPoints(Lifetime); }

  public virtual T?
    GetNearestSegment(Vector position, float since, int max = 0) {
    var nearest     = default(T);
    var minDistance = double.MaxValue;
    foreach (var segment in GetTrail(since, max)) {
      var distance = segment.GetDistanceSquared(position);
      if (distance >= minDistance) continue;
      minDistance = distance;
      nearest     = segment;
    }

    return nearest;
  }

  public virtual float GetTrailLength(float since, int max = 0) {
    return MathF.Sqrt(GetTrailLengthSquared(since, max));
  }

  public float GetTrailLengthSquared(float since, int max = 0) {
    var length = 0f;
    var last   = default(Vector);
    foreach (var segment in GetTrail(since, max)) {
      if (last != null) length += last.DistanceSquared(segment.GetStart());
      last = segment.GetEnd();
    }

    return length;
  }

  public T? GetNearestSegment(Vector position) {
    return GetNearestSegment(position, Lifetime);
  }

  public virtual void AddTrailPoint(Vector vector) {
    var start = GetEndSegment()?.GetEnd() ?? vector;
    var end   = vector;
    var seg   = CreateSegment(start, end);
    Segments.Insert(0, seg);
    Cleanup();
  }

  public abstract T CreateSegment(Vector start, Vector end);

  public virtual void Kill() {
    foreach (var segment in Segments) segment.Remove();

    Segments.Clear();
  }
}