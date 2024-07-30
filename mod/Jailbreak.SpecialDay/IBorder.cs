using CounterStrikeSharp.API.Modules.Utils;

namespace Jailbreak.SpecialDay;

public interface IBorder {
  bool IsInsiderBorder(Vector position);
  float GetMinDistance(Vector position);
  public Vector GetCenter();
  public IEnumerable<Vector> GetPoints();
}