using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.LastGuard;

public interface ILastGuardService {
  void StartLastGuard(CCSPlayerController lastGuard);

  public void DisableLastGuardForRound();
}