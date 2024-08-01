using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Public.Mod.Trail;

namespace Jailbreak.Trail;

public class VectorTrailSegment(Vector start, Vector end) : ITrailSegment {
  public readonly float spawnTime = Server.CurrentTime;

  public float GetSpawnTime() { return spawnTime; }

  public Vector GetStart() { return start; }

  public Vector GetEnd() { return end; }
  public virtual void Remove() { }
}