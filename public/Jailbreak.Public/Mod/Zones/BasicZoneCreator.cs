using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Mod.Zones;

public class BasicZoneCreator : IZoneCreator {
  protected List<Vector>? Points = [];

  public virtual void BeginCreation() { Points = []; }
  public virtual void AddPoint(Vector point) { Points?.Add(point); }

  public virtual void DeletePoint(Vector point) { Points?.Remove(point); }

  public virtual IZone Build(IZoneFactory factory) {
    if (Points == null) throw new InvalidOperationException("Points is null");
    var result = factory.CreateZone(Points);
    return result;
  }

  public virtual void Dispose() { Points = null; }
}