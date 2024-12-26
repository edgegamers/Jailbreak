using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.Public.Mod.LastRequest;

public abstract class AbstractLastRequest(BasePlugin plugin,
  ILastRequestManager manager, CCSPlayerController prisoner,
  CCSPlayerController guard) {
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

  // public bool PreventShoot(CCSPlayerController player, CBasePlayerWeapon weapon) {
  //   return false;
  // }
  //
  // public bool PreventPickup(CCSPlayerController player, CCSWeaponBase weapon) {
  //   return false;
  // }
}