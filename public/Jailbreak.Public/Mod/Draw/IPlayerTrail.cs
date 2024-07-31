using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.Public.Mod.Draw;

public interface IPlayerTrail {
  void SetUpdateRate(float rate);
  IList<Vector> GetTrail(float since, int max = 0);
  IList<Vector> GetTrail(int max) { return GetTrail(Lifetime, max); }
  IList<Vector> GetTrail() { return GetTrail(Lifetime); }
  ITrailSegment? GetNearestSegment(Vector position);

  float Lifetime { get; set; }
  int MaxPoints { get; set; }
}