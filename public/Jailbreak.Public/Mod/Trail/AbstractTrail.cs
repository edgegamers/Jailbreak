using System.Collections;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Extensions;

namespace Jailbreak.Public.Mod.Trail;

public abstract class AbstractTrail<T>(float lifetime = 20, int maxPoints = 100,
  float updateRate = 0.5f) : IEnumerable<T> where T : ITrailSegment {
  // Ordered from newest to oldest (0 is the newest)
  protected readonly IList<T> segments = new List<T>();

  public virtual float Lifetime {
    get => lifetime;
    set => lifetime = value;
  }

  private int MaxPoints { get; }

  public IEnumerator<T> GetEnumerator() { return segments.GetEnumerator(); }

  IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

  protected void Cleanup() {
    while (segments.Count > MaxPoints) segments.RemoveAt(segments.Count - 1);
  }

  public virtual IList<T> GetTrail(float since, int max = 0) {
    var result = new List<T>();
    foreach (var segment in segments) {
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

  public IList<Vector> GetTrailPoints(int max) {
    return GetTrailPoints(Lifetime, max);
  }

  public IList<Vector> GetTrailPoints() { return GetTrailPoints(Lifetime); }

  public T? GetNearestSegment(Vector position, float since, int max = 0) {
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

  public T? GetNearestSegment(Vector position) {
    ITrailSegment? nearest     = null;
    var            minDistance = double.MaxValue;
    foreach (var segment in segments) {
      var distance = segment.GetDistanceSquared(position);
      if (distance >= minDistance) continue;
      minDistance = distance;
      nearest     = segment;
    }

    return nearest;
  }

  public virtual void AddTrailPoint(Vector vector) {
    vector = vector.Clone();
    var mostRecent = segments.FirstOrDefault() ?? CreateSegment(vector, vector);
    if (mostRecent.GetEnd().Equals(vector)) return;
    segments.Insert(0, CreateSegment(mostRecent.GetEnd(), vector));
  }

  public abstract ITrailSegment CreateSegment(Vector start, Vector end);
}