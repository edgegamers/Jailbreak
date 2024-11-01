using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.LastGuard;

public interface ILastGuardService {
  bool IsLastGuardActive { get; }
  void StartLastGuard(CCSPlayerController lastGuard);

  public void DisableLastGuardForRound();
}