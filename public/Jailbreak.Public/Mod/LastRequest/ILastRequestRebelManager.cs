using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Behaviors;

namespace Jailbreak.Public.Mod.LastRequest;

public interface ILastRequestRebelManager : IPluginBehavior {
  public HashSet<int> PlayersLRRebelling { get; }
  public void StartLRRebelling(CCSPlayerController player);

  public bool IsInLRRebelling(int playerSlot) {
    return PlayersLRRebelling.Contains(playerSlot);
  }

  public void AddLRRebelling(int playerSlot) {
    PlayersLRRebelling.Add(playerSlot);
  }

  public void ClearLRRebelling() { PlayersLRRebelling.Clear(); }
}