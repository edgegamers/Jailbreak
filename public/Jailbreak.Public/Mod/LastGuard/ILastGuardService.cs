using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.LastGuard;

public interface ILastGuardService {
  [Obsolete("Unnecessary")]
  int CalculateHealth();

  void StartLastGuard(CCSPlayerController lastGuard);
}