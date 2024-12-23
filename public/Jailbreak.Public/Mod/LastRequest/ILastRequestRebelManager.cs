using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Behaviors;

namespace Jailbreak.Public.Mod.LastRequest;

public interface ILastRequestRebelManager : IPluginBehavior {
  public HashSet<int> PlayersLRRebelling { get; }
  public void MarkLRRebelling(CCSPlayerController player);
  public bool IsInLRRebelling(int playerSlot);
  public void AddLRRebelling(int playerSlot);
  public void ClearLRRebelling();
  public int CalculateHealth();
}