using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.Rebel;

namespace Jailbreak.LastRequest;

public class LastRequestRebelManager(IRebelService rebelService)
  : ILastRequestRebelManager {
  public HashSet<int> PlayersLRRebelling { get; } = new HashSet<int>();

  public void MarkLRRebelling(CCSPlayerController player) {
    AddLRRebelling(player.Slot);
    rebelService.MarkRebel(player);
  }

  public bool IsInLRRebelling(int playerSlot) {
    return PlayersLRRebelling.Contains(playerSlot);
  }

  public void AddLRRebelling(int playerSlot) {
    PlayersLRRebelling.Add(playerSlot);
  }

  public void ClearLRRebelling() {
    PlayersLRRebelling.Clear();
  }
}