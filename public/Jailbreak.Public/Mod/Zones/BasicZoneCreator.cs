using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Mod.Zones;

public class BasicZoneCreator : IZoneCreator {
  private List<Vector>? points = [];

  public virtual void BeginCreation() { points = []; }
  public virtual void AddPoint(Vector point) { points?.Add(point); }

  public virtual void DeletePoint(Vector point) { points?.Remove(point); }

  public virtual IZone Build(IZoneFactory factory) {
    if (points != null) {
      var result = factory.CreateZone(points);
      return result;
    }

    throw new InvalidOperationException("Points is null");
  }

  public virtual void Dispose() { points = null; }
}