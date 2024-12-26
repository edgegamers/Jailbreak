using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastRequest;
using Jailbreak.Public.Mod.LastRequest.Enums;
using Jailbreak.Public.Mod.Weapon;

namespace Jailbreak.LastRequest.LastRequests;

public abstract class TeleportingRequest(BasePlugin plugin,
  ILastRequestManager manager, CCSPlayerController prisoner,
  CCSPlayerController guard)
  : AbstractLastRequest(plugin, manager, prisoner, guard), IEquipBlocker {
  public override void Setup() {
    State = LRState.PENDING;

    Guard.Teleport(Prisoner);

    Guard.Freeze();
    Prisoner.Freeze();
    Plugin.AddTimer(1, () => { Guard.UnFreeze(); });
    Plugin.AddTimer(2, () => { Prisoner.UnFreeze(); });
  }

  public virtual bool PreventEquip(CCSPlayerController player, CCSWeaponBase weapon) {
    if (State == LRState.PENDING) return false;
    return player == Prisoner || player == Guard;
  }
}