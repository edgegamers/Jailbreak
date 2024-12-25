using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Jailbreak.Public.Extensions;
using Jailbreak.Public.Mod.LastRequest.Enums;

namespace Jailbreak.LastRequest.LastRequests;

public class KnifeFight(BasePlugin plugin, IServiceProvider provider,
  CCSPlayerController prisoner, CCSPlayerController guard)
  : WeaponizedRequest(plugin, provider, prisoner, guard) {
  public override LRType Type => LRType.KNIFE_FIGHT;

  public override void Execute() {
    Prisoner.GiveNamedItem("weapon_knife");
    Guard.GiveNamedItem("weapon_knife");
    State = LRState.ACTIVE;

    Plugin.RegisterListener<Listeners.OnTick>(restrictKnife);
  }

  private void restrictKnife() {
    disableWeapon(Prisoner);
    disableWeapon(Guard);
  }

  private void disableWeapon(CCSPlayerController player) {
    if (!player.IsReal()) return;
    var pawn = player.PlayerPawn.Value;
    if (pawn == null || !pawn.IsValid) return;
    var weaponServices = pawn.WeaponServices;
    if (weaponServices == null) return;
    var activeWeapon = weaponServices.ActiveWeapon.Value;
    if (activeWeapon == null || !activeWeapon.IsValid) return;
    if (activeWeapon.DesignerName.Contains("knife",
      StringComparison.OrdinalIgnoreCase))
      return;
    if (activeWeapon.DesignerName.Contains("bayonet",
      StringComparison.OrdinalIgnoreCase))
      return;
    activeWeapon.NextSecondaryAttackTick = Server.TickCount + 500;
    activeWeapon.NextPrimaryAttackTick   = Server.TickCount + 500;
  }

  public override void OnEnd(LRResult result) {
    Plugin.RegisterListener<Listeners.OnTick>(restrictKnife);
    State = LRState.COMPLETED;
  }
}