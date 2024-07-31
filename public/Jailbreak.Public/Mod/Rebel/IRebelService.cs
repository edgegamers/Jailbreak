using CounterStrikeSharp.API.Core;

namespace Jailbreak.Public.Mod.Rebel;

public interface IRebelService {
  ISet<CCSPlayerController> GetActiveRebels();

  bool IsRebel(CCSPlayerController player) {
    return GetRebelTimeLeft(player) > 0;
  }

  long GetRebelTimeLeft(CCSPlayerController player);

  bool MarkRebel(CCSPlayerController player, long time = -1);

  void UnmarkRebel(CCSPlayerController player);

  void DisableRebelForRound();
}