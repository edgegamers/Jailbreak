using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Mod.LastRequest.Enums;
using Jailbreak.Public.Mod.Weapon;

namespace Jailbreak.Public.Mod.LastRequest;

public abstract class AbstractLastRequest(BasePlugin plugin,
  ILastRequestManager manager, CCSPlayerController prisoner,
  CCSPlayerController guard) : IEquipBlocker {
  protected readonly ILastRequestManager Manager = manager;
  protected readonly BasePlugin Plugin = plugin;
  public CCSPlayerController Prisoner { get; protected set; } = prisoner;
  public CCSPlayerController Guard { get; protected set; } = guard;
  public abstract LRType Type { get; }

  public LRState State { get; protected set; }

  public void PrintToParticipants(string message) {
    Prisoner.PrintToChat(message);
    Guard.PrintToChat(message);
  }

  public abstract void Setup();
  public abstract void Execute();
  public abstract void OnEnd(LRResult result);

  public virtual bool PreventEquip(CCSPlayerController player,
    CCSWeaponBase weapon) {
    if (State == LRState.PENDING) return false;
    return player == Prisoner || player == Guard;
  }
}